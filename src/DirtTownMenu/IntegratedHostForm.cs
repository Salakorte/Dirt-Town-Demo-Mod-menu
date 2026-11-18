using System.Diagnostics;

namespace DirtTownMenu;

// External process only coordinates/injects. The interactive mod menu is drawn by the game.
internal sealed class IntegratedHostForm:Form
{
    readonly Label status=new(){Dock=DockStyle.Fill,Padding=new Padding(18),Text="Coordinador cooperativo opcional. El menú individual se carga con el juego.\nEntra en una sala con winmm.dll instalada. Se sincronizará automáticamente."};
    readonly System.Windows.Forms.Timer timer=new(){Interval=500};
    LiveBridge? bridge;
    Process? game;
    bool busy,closing;
    int checks;
    int attachFaultPid;
    uint handledEvent;
    string? pendingAck;
    public IntegratedHostForm()
    {
        Text="Dirt Town · Coordinador 0.6.3";ClientSize=new Size(500,160);Font=new Font("Segoe UI",10);Controls.Add(status);
        timer.Tick+=async(_,_)=>await Poll();timer.Start();
        FormClosing+=(_,e)=>{if(busy)e.Cancel=true;};
        FormClosed+=(_,_)=>{closing=true;timer.Dispose();bridge?.Dispose();game?.Dispose();};
    }
    async Task Poll()
    {
        if(busy||closing)return;busy=true;
        try
        {
            if(game is not null&&game.HasExited){bridge?.Dispose();bridge=null;game.Dispose();game=null;handledEvent=0;pendingAck=null;attachFaultPid=0;}
            if(bridge is null)
            {
                if(game is not null&&attachFaultPid==game.Id)return;
                var games=Process.GetProcessesByName("dirttown");if(games.Length!=1){foreach(var p in games)p.Dispose();return;}
                game=games[0];status.Text="Conectando al menú integrado del juego…";
                bridge=await Task.Run(()=>new LiveBridge(game));
            }
            if(++checks%4==0)await bridge.AutoSynchronize();
            using var info=await bridge.RequestRaw(8);var root=info.RootElement;
            if(root.GetProperty("pending").GetBoolean())
            {
                uint id=root.GetProperty("event").GetUInt32();
                if(id!=handledEvent)
                {
                    handledEvent=id;pendingAck="Resultado incierto. No repitas la creación; reinicia la sesión.";
                    try
                    {
                        if(root.GetProperty("command").GetUInt32()!=3)throw new IOException("Orden de menú desconocida.");
                        await bridge.AutoSynchronize();
                        using var result=await bridge.Request(3,x:root.GetProperty("x").GetSingle(),z:root.GetProperty("z").GetSingle(),model:root.GetProperty("model").GetString()!);
                        pendingAck="Vehiculo creado. "+bridge.CoopStatus;
                    }
                    catch(Exception ex){pendingAck=ex.Message;}
                }
                using var ack=await bridge.RequestRaw(9,vehicle:(int)id,payload:pendingAck);
            }
            string sync=bridge.CoopStatus;
            using var heartbeat=await bridge.RequestRaw(10,payload:sync);
            status.Text="Menú integrado conectado · F7 abrir/cerrar\nF8 llenar (configurable dentro del menú).\n"+sync+"\nMantén este coordinador abierto en ambos equipos.";
        }
        catch(Exception ex){if(bridge is null&&game is not null)attachFaultPid=game.Id;status.Text=ex.Message+"\n"+(bridge is null?"Reinicia el juego antes de reintentar la conexión.":"La partida debe estar cargada y sin pausa.");}
        finally{busy=false;}
    }
}
