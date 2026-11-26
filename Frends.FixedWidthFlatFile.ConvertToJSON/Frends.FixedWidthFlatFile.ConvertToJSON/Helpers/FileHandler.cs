using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Helpers;

/// <summary>
/// Reads the source file and writes the destination file.
/// </summary>
internal static class FileHandler
{
    internal static string ReadSource(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(path))
            throw new FileNotFoundException($"Flat file source '{path}' was not found.", path);

        // Strict UTF-8 (unless a BOM says otherwise) so undecodable bytes are rejected instead of replaced.
        try
        {
            using var reader = new StreamReader(path, new UTF8Encoding(false, true), true);
            var builder = new StringBuilder();
            var buffer = new char[8192];
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.Append(buffer, 0, read);
            }

            return builder.ToString();
        }
        catch (DecoderFallbackException ex)
        {
            throw new InvalidDataException($"Flat file source '{path}' is not valid UTF-8 text.", ex);
        }
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

    internal static void WriteNewFile(string fullPath, string content, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("Destination file path must include a directory.", nameof(fullPath));
        Directory.CreateDirectory(directory);

        cancellationToken.ThrowIfCancellationRequested();

        // CreateNew is an atomic create-if-absent and FileShare.None locks the file while it is written.
        FileStream stream;
        try
        {
            stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        }
        catch (IOException) when (File.Exists(fullPath))
        {
            throw new IOException($"Destination file '{fullPath}' already exists and will not be overwritten.");
        }

        try
        {
            using (stream)
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch
        {
            // Never leave a partial file behind.
            File.Delete(fullPath);
            throw;
        }
    }
}
