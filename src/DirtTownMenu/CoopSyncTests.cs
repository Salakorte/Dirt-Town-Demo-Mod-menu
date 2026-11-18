using System.Text.Json;
namespace DirtTownMenu;

internal static class CoopSyncTests
{
    sealed class Room
    {
        public readonly object Lock=new();
        public string Payload="";
        public readonly Dictionary<ulong,string> Data=new(){{1,""},{2,""}};
        public readonly Dictionary<ulong,List<string>> Fleet=new(){{1,["dozer","hauler"]},{2,["dozer","hauler"]}};
        public int HostCreates,GuestCreates;
        public bool Network=true,Available=true;
        public ulong Lobby=99;
        public bool CorruptPrepared,WrongGuestIndex,LeaveOnPrepare;
        public readonly Dictionary<ulong,List<int>> Owners=new(){{1,[7,2]},{2,[7,2]}};
    }
    sealed class Endpoint(Room room,ulong id):ICoopBackend
    {
        public Task<CoopState> Read()
        {
            lock(room.Lock)return Task.FromResult(new CoopState(room.Available,room.Network,room.Lobby,id,1,string.Join(',',room.Fleet[id]),room.Fleet[id].Count,room.Payload,room.Data.Select(p=>new CoopMember(p.Key,p.Value)).ToArray(),id==1?7:2));
        }
        public Task Publish(string value,bool member)
        {
            lock(room.Lock)
            {
                if(member)
                {
                    var ack=JsonSerializer.Deserialize<CoopAck>(value)!;
                    if(id==2&&room.CorruptPrepared&&ack.Phase=="prepared")value=JsonSerializer.Serialize(ack with{Count=ack.Count+1});
                    room.Data[id]=value;
                }
                else
                {
                    if(id!=1)throw new Exception("Guest published an authoritative order");room.Payload=value;
                    if(room.LeaveOnPrepare&&JsonSerializer.Deserialize<SpawnOrder>(value)!.Phase=="prepare")room.Data.Remove(2);
                }
            }
            return Task.CompletedTask;
        }
        public Task<int> Spawn(string model,float x,float z,int owner)
        {
            lock(room.Lock)
            {
                int index=room.Fleet[id].Count;room.Fleet[id].Add(model);
                room.Owners[id].Add(owner);
                if(id==1)room.HostCreates++;else room.GuestCreates++;
                return Task.FromResult(id==2&&room.WrongGuestIndex?index+1:index);
            }
        }
    }
    static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
    static async Task Fails(Func<Task> action)
    {try{await action();}catch(IOException){return;}catch(TimeoutException){return;}throw new Exception("Unsafe operation was not rejected");}
    public static async Task Run()
    {
        var room=new Room();var hostBackend=new Endpoint(room,1);var guestBackend=new Endpoint(room,2);
        using var host=new CoopSync(hostBackend,TimeSpan.FromSeconds(2),5);using var guest=new CoopSync(guestBackend,TimeSpan.FromSeconds(2),5);
        await host.Start();await guest.Start();
        int a=await host.Spawn("dozer",10,20);int b=await host.Spawn("hauler",30,40);
        Assert(a==2&&b==3&&room.HostCreates==2&&room.GuestCreates==2,"Peer vehicle IDs differ or creation was duplicated");
        Assert(room.Fleet[1].SequenceEqual(room.Fleet[2]),"Fleet order differs");
        Assert(room.Owners[1].SequenceEqual(new[]{7,2,7,7})&&room.Owners[1].SequenceEqual(room.Owners[2]),"Native host owner was not transmitted to both endpoints");
        await Task.Delay(60);Assert(room.GuestCreates==2,"Polling commit replayed creation");
        await Fails(async()=>await guest.Spawn("dozer",0,0));Assert(room.GuestCreates==2,"Guest authored an unauthorized creation");
        await guest.Stop();await host.Stop();
        Console.WriteLine("PASS: Two endpoints prepare/commit/ack, identical IDs and order, no replay, guest cannot author spawns.");

        var missing=new Room();using var h2=new CoopSync(new Endpoint(missing,1),TimeSpan.FromMilliseconds(100),5);await h2.Start();
        await Fails(async()=>await h2.Spawn("dozer",0,0));Assert(missing.HostCreates==0,"Host created before receiver was ready");await h2.Stop();
        Console.WriteLine("PASS: Missing receiver rejects creation before mutating host world.");

        var mismatch=new Room();mismatch.Fleet[2].Add("truck");
        using var h3=new CoopSync(new Endpoint(mismatch,1),TimeSpan.FromSeconds(1),5);using var g3=new CoopSync(new Endpoint(mismatch,2),TimeSpan.FromSeconds(1),5);
        await h3.Start();await g3.Start();await Fails(async()=>await h3.Spawn("dozer",0,0));
        Assert(mismatch.HostCreates==0&&mismatch.GuestCreates==0,"Mismatched fleets were mutated");await h3.Stop();await g3.Stop();
        Console.WriteLine("PASS: Different starting fleets reject creation without changing either endpoint.");

        var stale=new Room();stale.Payload=JsonSerializer.Serialize(new SpawnOrder(1,"old-commit","commit","dozer",1,2,"dozer,hauler",2,"dozer,hauler,dozer",3));
        using var g4=new CoopSync(new Endpoint(stale,2),TimeSpan.FromMilliseconds(100),5);await g4.Start();await Task.Delay(60);
        Assert(stale.GuestCreates==0,"Old lobby metadata created a stale vehicle after reconnect");await g4.Stop();
        Console.WriteLine("PASS: Restarted receiver ignores an old commit in lobby metadata.");

        var paused=new Room();paused.Data[2]=JsonSerializer.Serialize(new CoopAck(1,"","ready","dozer,hauler",2));
        using var h5=new CoopSync(new Endpoint(paused,1),TimeSpan.FromMilliseconds(100),5);await h5.Start();
        await Fails(async()=>await h5.Spawn("dozer",0,0));Assert(paused.HostCreates==0,"Prepare timeout created an invisible vehicle");await h5.Stop();
        Assert(paused.Payload.Contains("abort"),"Timed out prepare was not aborted");
        Console.WriteLine("PASS: Offline/paused receiver times out during prepare, aborts and creates nothing.");

        var corrupt=new Room{CorruptPrepared=true};
        using var h6=new CoopSync(new Endpoint(corrupt,1),TimeSpan.FromSeconds(1),5);using var g6=new CoopSync(new Endpoint(corrupt,2),TimeSpan.FromSeconds(1),5);
        await h6.Start();await g6.Start();await Fails(async()=>await h6.Spawn("dozer",0,0));
        Assert(corrupt.HostCreates==0&&corrupt.GuestCreates==0,"Wrong prepared count was accepted");await h6.Stop();await g6.Stop();
        Console.WriteLine("PASS: Prepared acknowledgement must match the exact fleet fingerprint and count.");

        var partial=new Room{WrongGuestIndex=true};
        using var h7=new CoopSync(new Endpoint(partial,1),TimeSpan.FromSeconds(1),5);using var g7=new CoopSync(new Endpoint(partial,2),TimeSpan.FromSeconds(1),5);
        await h7.Start();await g7.Start();await Fails(async()=>await h7.Spawn("dozer",0,0));await Fails(async()=>await h7.Spawn("dozer",0,0));
        Assert(partial.HostCreates==1&&partial.GuestCreates==1,"An uncertain commit was retried and duplicated");await h7.Stop();await g7.Stop();
        Console.WriteLine("PASS: Wrong committed index stops creation and cannot be retried into duplicates.");

        var leaving=new Room{LeaveOnPrepare=true};using var h8=new CoopSync(new Endpoint(leaving,1),TimeSpan.FromSeconds(1),5);using var g8=new CoopSync(new Endpoint(leaving,2),TimeSpan.FromSeconds(1),5);
        await h8.Start();await g8.Start();await Fails(async()=>await h8.Spawn("dozer",0,0));
        Assert(leaving.HostCreates==0,"Host created after a peer left during preparation");await h8.Stop();await g8.Stop();
        Console.WriteLine("PASS: Membership change during preparation cancels before host mutation.");

        var autoRoom=new Room{Network=false};using var auto=new AutoCoopSession(new Endpoint(autoRoom,1));
        await auto.Refresh();Assert(auto.Current is null,"Automatic sync activated in single player");
        autoRoom.Network=true;await auto.Refresh();var first=auto.Current;Assert(first is not null,"Coop room did not activate automatically");
        await auto.Refresh();Assert(ReferenceEquals(first,auto.Current),"Polling recreated an active session");
        using var autoGuest=new CoopSync(new Endpoint(autoRoom,2),TimeSpan.FromSeconds(2),5);await autoGuest.Start();autoRoom.WrongGuestIndex=true;
        await Fails(async()=>await auto.Current!.Spawn("dozer",0,0));await auto.Refresh();
        Assert(ReferenceEquals(first,auto.Current),"Automatic refresh cleared the partial-spawn fault");
        await Fails(async()=>await auto.Current!.Spawn("dozer",0,0));Assert(autoRoom.HostCreates==1&&autoRoom.GuestCreates==1,"Automatic sync duplicated an uncertain creation");await autoGuest.Stop();
        autoRoom.Lobby=100;await auto.Refresh();Assert(auto.Current is not null&&!ReferenceEquals(first,auto.Current),"Changed room did not reconnect");
        autoRoom.Network=false;await auto.Refresh();Assert(auto.Current is null,"Sync stayed active after leaving coop");await auto.Stop();
        Console.WriteLine("PASS: Automatic detection, stable polling, fault retention without duplicates, room reconnection and single-player stop.");
    }
}
