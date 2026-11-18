namespace DirtTownMenu;

internal sealed class AutoCoopSession(ICoopBackend backend):IDisposable
{
    readonly SemaphoreSlim gate=new(1);
    ulong lobby,owner;
    public CoopSync? Current {get;private set;}
    public string Status=>Current?.Status??"Individual o esperando una sala cooperativa compatible";
    public async Task Refresh()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var state=await backend.Read().ConfigureAwait(false);
            if(Current is not null&&(!state.Available||!state.Network||state.Lobby!=lobby||state.Owner!=owner))
            {var previous=Current;Current=null;await previous.Stop().ConfigureAwait(false);previous.Dispose();}
            if(Current is null&&state.Available&&state.Network&&state.Lobby!=0)
            {
                var candidate=new CoopSync(backend);
                try{await candidate.Start().ConfigureAwait(false);Current=candidate;lobby=state.Lobby;owner=state.Owner;}
                catch{candidate.Dispose();throw;}
            }
            // A faulted session is deliberately retained until a real room/network change.
            // Recreating it on every poll would bypass the no-retry protection after a partial spawn.
        }
        finally{gate.Release();}
    }
    public async Task Stop()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try{if(Current is not null){await Current.Stop().ConfigureAwait(false);Current.Dispose();Current=null;}}
        finally{gate.Release();}
    }
    public void Dispose(){Current?.Dispose();}
}
