using System.Text.Json;

namespace DirtTownMenu;

internal record CoopAck(int Version,string Token,string Phase,string Roster,int Count,string? Error=null);
internal record CoopMember(ulong Id,string Data);
internal record CoopState(bool Available,bool Network,ulong Lobby,ulong Self,ulong Owner,string Roster,int Count,string Payload,CoopMember[] Members,int NativePlayer=0);
internal record SpawnOrder(int Version,string Token,string Phase,string Model,float X,float Z,string Before,int BeforeCount,string After="",int AfterCount=0,int NativeOwner=0);
internal interface ICoopBackend
{
    Task<CoopState> Read();
    Task Publish(string value,bool member);
    Task<int> Spawn(string model,float x,float z,int owner);
}

internal sealed class CoopSync:IDisposable
{
    readonly ICoopBackend backend;
    readonly CancellationTokenSource stop=new();
    readonly SemaphoreSlim serial=new(1);
    readonly HashSet<string> seen=[];
    readonly TimeSpan timeout;
    readonly int interval;
    CoopState initial=null!;
    SpawnOrder? prepared;
    CoopAck? ack;
    Task? loop;
    bool failed;
    public string Status {get;private set;}="Sin activar";
    public bool IsHost=>initial.Self==initial.Owner;
    static readonly JsonSerializerOptions Json=new(){PropertyNameCaseInsensitive=true};
    public CoopSync(ICoopBackend backend,TimeSpan? timeout=null,int interval=750)
    {this.backend=backend;this.timeout=timeout??TimeSpan.FromSeconds(40);this.interval=interval;}

    public async Task Start()
    {
        initial=await backend.Read().ConfigureAwait(false);
        if(!initial.Available||!initial.Network||initial.Lobby==0||initial.Count<1)throw new IOException("Entra primero en una partida cooperativa con una sala compatible.");
        // Existing lobby data must never replay when a receiver is restarted or joins late.
        var old=Parse<SpawnOrder>(initial.Payload);if(old is not null)seen.Add(old.Token);
        await SetAck(new(1,"","ready",initial.Roster,initial.Count)).ConfigureAwait(false);
        Status=IsHost?"Anfitrión · receptor activo":"Invitado · receptor activo; crea los vehículos desde el anfitrión";
        loop=Task.Run(Loop);
    }
    static T? Parse<T>(string data) where T:class
    {try{return JsonSerializer.Deserialize<T>(data,Json);}catch(JsonException){return null;}}
    void Validate(CoopState state)
    {
        if(!state.Available||!state.Network||state.Lobby!=initial.Lobby||state.Owner!=initial.Owner||state.Self!=initial.Self)
            throw new IOException("La sala o el anfitrión ha cambiado. Reconecta la sincronización antes de continuar.");
    }
    async Task SetAck(CoopAck value)
    {
        if(ack==value)return;
        await backend.Publish(JsonSerializer.Serialize(value),true).ConfigureAwait(false);ack=value;
    }
    async Task Loop()
    {
        try
        {
            while(!stop.IsCancellationRequested)
            {
                await Task.Delay(interval,stop.Token).ConfigureAwait(false);
                await serial.WaitAsync(stop.Token).ConfigureAwait(false);
                try
                {
                    var state=await backend.Read().ConfigureAwait(false);Validate(state);
                    if(!IsHost&&!failed)await Receive(state).ConfigureAwait(false);
                    else if(!failed&&ack?.Phase is "ready" or "applied")await SetAck(new(1,ack.Token,ack.Phase,state.Roster,state.Count)).ConfigureAwait(false);
                }
                finally{serial.Release();}
            }
        }
        catch(OperationCanceledException){}
        catch(Exception ex){failed=true;Status="Sincronización detenida: "+ex.Message;}
    }
    async Task Receive(CoopState state)
    {
        var order=Parse<SpawnOrder>(state.Payload);
        if(order is null||order.Version!=1||string.IsNullOrEmpty(order.Token))return;
        if(seen.Contains(order.Token))return;
        if(order.Phase=="abort")
        {
            seen.Add(order.Token);prepared=null;await SetAck(new(1,order.Token,"ready",state.Roster,state.Count)).ConfigureAwait(false);return;
        }
        if(order.Phase=="prepare")
        {
            if(order.Before!=state.Roster||order.BeforeCount!=state.Count)
            {await SetAck(new(1,order.Token,"error",state.Roster,state.Count,"Las listas de vehículos no coinciden")).ConfigureAwait(false);return;}
            if(!ValidModel(order.Model)||!float.IsFinite(order.X)||!float.IsFinite(order.Z)||Math.Abs(order.X)>10000||Math.Abs(order.Z)>10000)
            {await SetAck(new(1,order.Token,"error",state.Roster,state.Count,"Modelo o posición no válidos")).ConfigureAwait(false);return;}
            if(order.NativeOwner<0||order.NativeOwner>1024)throw new IOException("Propietario nativo no válido");
            prepared=order;
            await SetAck(new(1,order.Token,"prepared",state.Roster,state.Count)).ConfigureAwait(false);
        }
        else if(order.Phase=="commit")
        {
            // Mark before calling the game: even an uncertain failure must not create duplicates.
            seen.Add(order.Token);
            try
            {
                if(prepared is null||prepared!=order with{Phase="prepare",After="",AfterCount=0}||order.Before!=state.Roster||order.BeforeCount!=state.Count)
                    throw new IOException("La creación no estaba preparada o la flota ha cambiado");
                int id=await backend.Spawn(order.Model,order.X,order.Z,order.NativeOwner).ConfigureAwait(false);
                var after=await backend.Read().ConfigureAwait(false);Validate(after);
                if(id!=order.BeforeCount||after.Count!=order.BeforeCount+1||after.Roster!=order.After||after.Count!=order.AfterCount)throw new IOException("El vehículo creado no coincide con el del anfitrión");
                await SetAck(new(1,order.Token,"applied",after.Roster,after.Count)).ConfigureAwait(false);
                Status="Vehículo recibido del anfitrión: "+order.Model;prepared=null;
            }
            catch(Exception ex)
            {
                failed=true;Status="Error de sincronización: "+ex.Message;
                await SetAck(new(1,order.Token,"error",state.Roster,state.Count,ex.Message)).ConfigureAwait(false);
            }
        }
    }
    static bool ValidModel(string model)=>new[]{"dozer","hauler","bandvagn","truck","excavator","large_loader","medium_loader","tractor","dirtbike","snowmobile","hooklift","feeder"}.Contains(model);
    static ulong[] Members(CoopState state)=>state.Members.Select(m=>m.Id).Order().ToArray();
    public async Task<int> Spawn(string model,float x,float z)
    {
        await serial.WaitAsync(stop.Token).ConfigureAwait(false);
        bool mutated=false;SpawnOrder? order=null;
        try
        {
            if(failed)throw new IOException("La sincronización está detenida. No repitas la creación; reinicia la sesión para recuperar una flota común.");
            if(!IsHost)throw new IOException("En esta versión los vehículos sincronizados se crean desde el anfitrión.");
            if(!ValidModel(model)||!float.IsFinite(x)||!float.IsFinite(z)||Math.Abs(x)>10000||Math.Abs(z)>10000)throw new IOException("Modelo o posición no válidos.");
            var state=await backend.Read().ConfigureAwait(false);Validate(state);
            foreach(var peer in state.Members.Where(m=>m.Id!=state.Self))
            {
                var ready=Parse<CoopAck>(peer.Data);
                if(ready is null||ready.Version!=1||ready.Phase is not ("ready" or "applied")||ready.Roster!=state.Roster||ready.Count!=state.Count)
                    throw new IOException("Falta un receptor compatible o las flotas no coinciden. Ambos deben conectar la misma versión del menú en una sesión con la misma flota inicial.");
            }
            if(state.Members.Length<2)throw new IOException("Espera a que el invitado entre y active la sincronización.");
            if(state.NativePlayer<0||state.NativePlayer>1024)throw new IOException("El anfitrión no tiene un identificador de jugador nativo válido.");
            order=new(1,Guid.NewGuid().ToString("N"),"prepare",model,x,z,state.Roster,state.Count,NativeOwner:state.NativePlayer);
            Status="Esperando que todos preparen el vehículo…";
            await backend.Publish(JsonSerializer.Serialize(order),false).ConfigureAwait(false);
            await WaitFor(order,"prepared",state).ConfigureAwait(false);
            mutated=true;
            int id=await backend.Spawn(model,x,z,order.NativeOwner).ConfigureAwait(false);
            var after=await backend.Read().ConfigureAwait(false);Validate(after);
            if(id!=state.Count||after.Count!=state.Count+1)throw new IOException("El juego no ha añadido exactamente un vehículo al final de la lista.");
            order=order with {Phase="commit",After=after.Roster,AfterCount=after.Count};
            await backend.Publish(JsonSerializer.Serialize(order),false).ConfigureAwait(false);
            Status="Vehículo creado; esperando confirmación del invitado…";
            await WaitFor(order,"applied",state).ConfigureAwait(false);
            await SetAck(new(1,order.Token,"applied",after.Roster,after.Count)).ConfigureAwait(false);
            Status="Creación confirmada en ambos equipos";return id;
        }
        catch(Exception ex)
        {
            if(mutated){failed=true;Status="Creación incompleta; no repetir: "+ex.Message;throw new IOException(Status,ex);}
            if(order is not null)try{await backend.Publish(JsonSerializer.Serialize(order with{Phase="abort"}),false).ConfigureAwait(false);}catch{}
            Status="No se ha creado el vehículo: "+ex.Message;throw;
        }
        finally{serial.Release();}
    }
    async Task WaitFor(SpawnOrder order,string phase,CoopState before)
    {
        var deadline=DateTime.UtcNow+timeout;
        while(DateTime.UtcNow<deadline)
        {
            stop.Token.ThrowIfCancellationRequested();
            var state=await backend.Read().ConfigureAwait(false);Validate(state);
            if(!Members(state).SequenceEqual(Members(before)))throw new IOException("Ha cambiado la lista de jugadores durante la creación.");
            bool all=true;
            foreach(var member in state.Members.Where(m=>m.Id!=state.Self))
            {
                var value=Parse<CoopAck>(member.Data);
                if(value?.Token==order.Token&&value.Phase=="error")throw new IOException(value.Error??"El invitado ha rechazado la operación.");
                if(value?.Token!=order.Token||value.Version!=1||value.Phase!=phase)all=false;
                else if(value.Roster!=(phase=="prepared"?order.Before:order.After)||value.Count!=(phase=="prepared"?order.BeforeCount:order.AfterCount))
                    throw new IOException("El invitado ha confirmado una lista de vehículos diferente.");
            }
            if(all)return;
            await Task.Delay(interval,stop.Token).ConfigureAwait(false);
        }
        throw new TimeoutException("El invitado no ha confirmado. Debe mantener el receptor conectado y la partida sin pausa.");
    }
    public async Task Stop()
    {
        stop.Cancel();if(loop is not null)await loop.ConfigureAwait(false);
        if(initial is not null)try{await SetAck(new(1,"","offline",initial.Roster,initial.Count)).ConfigureAwait(false);}catch{}
    }
    public void Dispose(){stop.Cancel();}
}

internal sealed class NativeCoopBackend(LiveBridge bridge):ICoopBackend
{
    public async Task<CoopState> Read()
    {
        using var doc=await bridge.RequestRaw(6).ConfigureAwait(false);var v=doc.RootElement;
        bool available=v.GetProperty("available").GetBoolean();
        return new(available,v.GetProperty("network").GetBoolean(),available?v.GetProperty("lobby").GetUInt64():0,available?v.GetProperty("self").GetUInt64():0,available?v.GetProperty("owner").GetUInt64():0,v.GetProperty("roster").GetString()!,v.GetProperty("count").GetInt32(),available?v.GetProperty("payload").GetString()!:"",available?v.GetProperty("members").EnumerateArray().Select(m=>new CoopMember(m.GetProperty("id").GetUInt64(),m.GetProperty("data").GetString()!)).ToArray():[],v.GetProperty("nativePlayer").GetInt32());
    }
    public async Task Publish(string value,bool member){using var doc=await bridge.RequestRaw(7,vehicle:member?1:0,payload:value).ConfigureAwait(false);}
    public async Task<int> Spawn(string model,float x,float z,int owner){using var doc=await bridge.RequestRaw(3,vehicle:owner,x:x,z:z,model:model).ConfigureAwait(false);return doc.RootElement.GetProperty("vehicle").GetInt32();}
}
