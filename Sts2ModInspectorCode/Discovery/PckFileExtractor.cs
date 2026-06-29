using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sts2ModInspector.Discovery;

// Lean port of Sts2SkinManager's PckFileExtractor — only the directory-index reader is needed
// here (we count entries + sum their declared sizes to estimate a mod's pck footprint; we never
// extract file bytes). Reads a Godot 4 PCK directory without mounting it.
public sealed class PckEntry
{
    public string Path { get; init; } = "";
    public long Size { get; init; }
}

public static class PckFileExtractor
{
    private const uint MagicGdpc = 0x43504447; // "GDPC" LE
    private const int TailScanWindow = 512 * 1024;

    private static readonly string[] PathPrefixHints =
    {
        ".godot/", "animations/", "images/", "card_", "res://", "characters/", "scripts/",
    };

    public static List<PckEntry>? TryReadIndex(string pckPath)
    {
        if (!File.Exists(pckPath)) return null;
        try
        {
            using var fs = new FileStream(pckPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);

            if (br.ReadUInt32() != MagicGdpc) return null;

            var packFormat = br.ReadUInt32();
            br.ReadUInt32(); br.ReadUInt32(); br.ReadUInt32();    // godot version triple

            long fileBase = 0;
            long headerDirOffset = 0;
            if (packFormat >= 2)
            {
                br.ReadUInt32();              // pack flags
                fileBase = br.ReadInt64();
                headerDirOffset = br.ReadInt64();
            }

            var dirStart = -1L;
            if (headerDirOffset > 0 && headerDirOffset + 8 <= fs.Length && LooksLikeDirectory(fs, br, headerDirOffset))
                dirStart = headerDirOffset;
            if (dirStart < 0) dirStart = LocateDirectoryStart(fs, fileBase);
            if (dirStart < 0) return null;

            fs.Seek(dirStart, SeekOrigin.Begin);
            var fileCount = br.ReadUInt32();
            if (fileCount > 1_000_000) return null;

            var entries = new List<PckEntry>((int)Math.Min(fileCount, 8192));
            for (uint i = 0; i < fileCount; i++)
            {
                var pathLen = br.ReadUInt32();
                if (pathLen > 4096) return null;
                var pathBytes = br.ReadBytes((int)pathLen);

                var actualLen = pathBytes.Length;
                while (actualLen > 0 && pathBytes[actualLen - 1] == 0) actualLen--;
                var path = Encoding.UTF8.GetString(pathBytes, 0, actualLen);

                br.ReadInt64();                                    // file offset (relative) — unused
                var fileSize = br.ReadInt64();
                br.ReadBytes(16);                                  // md5
                if (packFormat >= 2) br.ReadUInt32();              // per-entry flags

                entries.Add(new PckEntry { Path = path, Size = fileSize });
            }
            return entries;
        }
        catch
        {
            return null;
        }
    }

    private static bool LooksLikeDirectory(FileStream fs, BinaryReader br, long offset)
    {
        try
        {
            fs.Seek(offset, SeekOrigin.Begin);
            var fileCount = br.ReadUInt32();
            if (fileCount < 1 || fileCount > 1_000_000) return false;
            var plen = br.ReadUInt32();
            if (plen < 4 || plen > 4096 || plen % 4 != 0) return false;
            var pathBytes = br.ReadBytes((int)plen);
            if (pathBytes.Length != plen) return false;
            foreach (var b in pathBytes)
                if (b != 0 && (b < 0x20 || b > 0x7e)) return false;
            return true;
        }
        catch { return false; }
    }

    private static long LocateDirectoryStart(FileStream fs, long fileBase)
    {
        var len = fs.Length;
        if (len < 128) return -1;

        var windowSize = (int)Math.Min(TailScanWindow, Math.Max(0, len - fileBase));
        if (windowSize < 64) return -1;

        var windowBase = len - windowSize;
        fs.Seek(windowBase, SeekOrigin.Begin);
        var buf = new byte[windowSize];
        var total = 0;
        while (total < windowSize)
        {
            var read = fs.Read(buf, total, windowSize - total);
            if (read <= 0) break;
            total += read;
        }
        if (total != windowSize) return -1;

        var maxStartInBuf = windowSize - 12;
        for (var o = maxStartInBuf; o >= 0; o -= 4)
        {
            var fcount = BitConverter.ToUInt32(buf, o);
            if (fcount < 1 || fcount > 50_000) continue;

            var plen = BitConverter.ToUInt32(buf, o + 4);
            if (plen < 4 || plen > 512 || plen % 4 != 0) continue;
            if (o + 8 + plen > buf.Length) continue;

            var actLen = (int)plen;
            while (actLen > 0 && buf[o + 8 + actLen - 1] == 0) actLen--;
            if (actLen == 0) continue;

            string pathStr;
            try { pathStr = Encoding.UTF8.GetString(buf, o + 8, actLen); }
            catch { continue; }

            var matched = false;
            foreach (var hint in PathPrefixHints)
            {
                if (pathStr.StartsWith(hint, StringComparison.Ordinal)) { matched = true; break; }
            }
            if (!matched) continue;

            return windowBase + o;
        }
        return -1;
    }
}
