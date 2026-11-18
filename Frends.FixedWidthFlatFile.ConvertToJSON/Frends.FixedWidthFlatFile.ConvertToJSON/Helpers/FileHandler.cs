using System;
using System.IO;
using System.Text;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Helpers;

/// <summary>
/// Reads the source file and writes the destination file.
/// </summary>
internal static class FileHandler
{
    internal static string ReadSource(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Flat file source '{path}' was not found.", path);

        return File.ReadAllText(path);
    }

    internal static string ResolveDestinationPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("Destination file path is not a valid path.");
        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException("Destination file path must be a fully qualified absolute path.");
        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
            throw new ArgumentException("Destination file path must point to a file, not a directory.");

        var fullPath = Path.GetFullPath(path);
        if (Directory.Exists(fullPath))
            throw new IOException($"Destination path '{fullPath}' is an existing directory.");
        return File.Exists(fullPath)
            ? throw new IOException($"Destination file '{fullPath}' already exists and will not be overwritten.")
            : fullPath;
    }

    internal static void WriteNewFile(string fullPath, string content)
    {
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("Destination file path must include a directory.", nameof(fullPath));
        Directory.CreateDirectory(directory);

        // Write to a temp file first so a failed write never leaves a partial file at the final path.
        var tempPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }

            // Move without overwrite throws if the file appeared after validation.
            File.Move(tempPath, fullPath, false);
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }
    }
}
