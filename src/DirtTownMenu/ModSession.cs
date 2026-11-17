using System.Diagnostics;
using System.Text.Json;

namespace DirtTownMenu;

internal sealed class ModSession : IDisposable
{
    readonly RemoteMemory memory;
    readonly Dictionary<long,byte[]> owned = [];
    readonly long moduleBase;
    public string BackupPath { get; }
    public int ProcessId { get; }
    public bool HasExited => memory.HasExited;

    public ModSession(Process target, string backupRoot)
    {
        string path = target.MainModule?.FileName ?? throw new InvalidOperationException("No se pudo leer el ejecutable del juego.");
        if (!Path.GetFileName(path).Equals("dirttown.exe",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Proceso de juego incorrecto.");
        BuildProfile.ValidateFile(path);
        ProcessId = target.Id;
        moduleBase = target.MainModule!.BaseAddress.ToInt64();
        memory = new RemoteMemory(target);
        try
        {
            foreach (var patch in BuildProfile.Unlock.Concat(BuildProfile.Range(10)))
            {
                if (!memory.Read(moduleBase+patch.Rva,patch.Original.Length).SequenceEqual(patch.Original))
                    throw new InvalidOperationException("Otro programa ha cambiado instrucciones que usa el menú. Esta combinación necesita un ajuste de compatibilidad; no se han sobrescrito sus cambios.");
                owned.Add(patch.Rva,patch.Original.ToArray());
            }
            BackupPath = CreateBackup(backupRoot);
        }
        catch { memory.Dispose(); throw; }
    }

    public static string CreateBackup(string root)
    {
        string source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Dirt Town Demo");
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException("No se encuentra la carpeta de partidas. Abre el juego una vez antes de conectar.");
        string destination=Path.Combine(root,DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.EnumerateFiles(source,"*",SearchOption.AllDirectories))
        {
            var info=new FileInfo(file);
            // Saves, configuration and existing backup files; don't duplicate large crash dumps or active logs.
            string relative=Path.GetRelativePath(source,file);
            if (relative.Split(Path.DirectorySeparatorChar).Any(part => part.Equals("dumps",StringComparison.OrdinalIgnoreCase) || part.Equals("log",StringComparison.OrdinalIgnoreCase))) continue;
            if (Path.GetExtension(file).Equals(".log",StringComparison.OrdinalIgnoreCase)) continue;
            string to=Path.Combine(destination,relative);
            Directory.CreateDirectory(Path.GetDirectoryName(to)!);
            File.Copy(file,to,false);
            if (new FileInfo(to).Length!=info.Length) throw new IOException("Una partida cambió durante la copia. Vuelve al menú principal y conecta otra vez.");
        }
        File.WriteAllText(Path.Combine(destination,"backup-info.json"),JsonSerializer.Serialize(new {source,created=DateTimeOffset.Now,build=BuildProfile.SteamBuild},new JsonSerializerOptions {WriteIndented=true}));
        return destination;
    }

    public void SetUnlock(bool enabled) => Apply(BuildProfile.Unlock,enabled);
    public void SetRange(int metres) => Apply(BuildProfile.Range(metres==0?10:metres),metres!=0);

    void Apply(PatchSite[] patches, bool enabled)
    {
        using var frozen=memory.Freeze();
        // Validate all sites before making any write; preserve the exact previous setting on rollback.
        var before=patches.ToDictionary(p=>p.Rva,p=>memory.Read(moduleBase+p.Rva,p.Original.Length));
        foreach(var patch in patches)
            if(!before[patch.Rva].SequenceEqual(owned[patch.Rva]))
                throw new InvalidOperationException("Se ha detectado un cambio externo. No se sobrescribirá. Cierra y vuelve a abrir el juego.");
        var changed=new List<long>();
        try
        {
            foreach(var patch in patches)
            {
                changed.Add(patch.Rva);
                memory.Write(moduleBase+patch.Rva,enabled?patch.Enabled:patch.Original);
            }
            foreach(var patch in patches) owned[patch.Rva]=(enabled?patch.Enabled:patch.Original).ToArray();
        }
        catch(Exception error)
        {
            var rollbackErrors=new List<Exception>();
            foreach(long rva in changed.AsEnumerable().Reverse())
                try { memory.Write(moduleBase+rva,before[rva]); } catch(Exception ex) {rollbackErrors.Add(ex);}
            if(rollbackErrors.Count>0) throw new AggregateException("No se pudo deshacer todo el cambio. Cierra el juego antes de continuar.",rollbackErrors.Prepend(error));
            throw;
        }
    }

    public void Restore()
    {
        if(HasExited) return;
        // A single transaction restores all four instructions, including a previously selected range.
        Apply(BuildProfile.Unlock.Concat(BuildProfile.Range(10)).ToArray(),false);
    }
    public void Dispose() => memory.Dispose();
}
