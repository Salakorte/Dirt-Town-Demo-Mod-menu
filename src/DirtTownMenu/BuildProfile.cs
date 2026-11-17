using System.Security.Cryptography;

namespace DirtTownMenu;

internal record PatchSite(long Rva, byte[] Original, byte[] Enabled);

internal static class BuildProfile
{
    public const string Hash = "D8FDDD701778B9C4E19FFEEDC68243D49BAC983C87E6900CC6D14F6997C0E898";
    public const string SteamBuild = "26690600";
    public const string DefaultGame = @"C:\Program Files (x86)\Steam\steamapps\common\Dirt Town Demo\dirttown.exe";

    // All sites inspect MachineSpec.locked's string length at offset 0x170.
    // Keep ownership, range, type and occupancy checks. In addition to entering,
    // the control-selection mask must mark the machine as driveable.
    public static readonly PatchSite[] Unlock = [
        new(0x947DE0, Convert.FromHexString("0F8572010000"), Convert.FromHexString("909090909090")),
        new(0x953675, Convert.FromHexString("7529"), Convert.FromHexString("9090")),
        // Eligibility helper used by vehicle selection.
        new(0x951C2A, Convert.FromHexString("740C"), Convert.FromHexString("EB0C")),
        // Per-machine control mask, consumed by the local driver selection path.
        new(0x5ADF6B, Convert.FromHexString("0F94442420"), Convert.FromHexString("C644242001")),
        // Secondary drive/availability helper; do not skip its ownership tests.
        new(0x5ADC68, Convert.FromHexString("7436"), Convert.FromHexString("EB36")),
        // Restoring/selecting a saved seat must also allow the unlocked machine.
        new(0x5E3966, Convert.FromHexString("7530"), Convert.FromHexString("9090"))
    ];

    public static PatchSite[] Range(int metres)
    {
        long constant = metres switch { 10 => 0x1485CD0, 25 => 0x1481088, 50 => 0x1492D64,
            _ => throw new ArgumentOutOfRangeException(nameof(metres)) };
        return new long[] { 0x953599, 0x9537B1 }.Select(rva =>
        {
            // vmovss xmm6,[rsi+0xd424] -> vmovss xmm6,[rip+existingFloat].
            byte[] replacement = Convert.FromHexString("C5FA103500000000");
            BitConverter.GetBytes(checked((int)(constant-rva-8))).CopyTo(replacement, 4);
            return new PatchSite(rva, Convert.FromHexString("C5FA10B624D40000"), replacement);
        }).ToArray();
    }

    public static void ValidateFile(string path)
    {
        using var file = File.OpenRead(path);
        if (Convert.ToHexString(SHA256.HashData(file)) != Hash)
            throw new InvalidOperationException("Esta versión del juego no es compatible. Se requiere la build 26690600. No se ha aplicado ningún cambio.");
        using var reader = new BinaryReader(file);
        file.Position = 0x3c;
        int nt = reader.ReadInt32();
        file.Position = nt+6;
        int count = reader.ReadUInt16();
        file.Position = nt+20;
        int optionalSize = reader.ReadUInt16();
        var sections = new List<(uint Rva,uint Size,uint Raw)>();
        file.Position = nt+24+optionalSize;
        for (int i=0; i<count; i++)
        {
            file.Position += 8;
            _ = reader.ReadUInt32();
            uint rva = reader.ReadUInt32(), size = reader.ReadUInt32(), raw = reader.ReadUInt32();
            sections.Add((rva,size,raw));
            file.Position += 16;
        }
        byte[] At(long rva, int size)
        {
            var s = sections.Single(s => rva>=s.Rva && rva+size<=s.Rva+s.Size);
            file.Position = s.Raw+rva-s.Rva;
            return reader.ReadBytes(size);
        }
        foreach (var patch in Unlock.Concat(Range(10)))
            if (!At(patch.Rva,patch.Original.Length).SequenceEqual(patch.Original))
                throw new InvalidDataException("Los bytes de la build no coinciden con el perfil.");
        foreach (int value in new[] {10,25,50})
            foreach (var patch in Range(value))
            {
                long target = patch.Rva+8+BitConverter.ToInt32(patch.Enabled,4);
                if (BitConverter.ToSingle(At(target,4)) != value)
                    throw new InvalidDataException("Constante de distancia incorrecta.");
            }
    }
}
