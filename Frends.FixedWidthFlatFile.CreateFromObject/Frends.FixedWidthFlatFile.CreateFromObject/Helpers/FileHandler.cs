using System.Text;

namespace Frends.FixedWidthFlatFile.CreateFromObject.Helpers;

internal static class FileHandler
{
    internal static string ResolveDestinationPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException("Destination file path is not a valid path.", nameof(path));
        }

        if (!Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("Destination file path must be a fully qualified absolute path.", nameof(path));
        }

        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException("Destination file path must point to a file, not a directory.", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);
        if (Directory.Exists(fullPath))
        {
            throw new IOException($"Destination path '{fullPath}' is an existing directory.");
        }

        if (File.Exists(fullPath))
        {
            throw new IOException($"Destination file '{fullPath}' already exists and will not be overwritten.");
        }

        return fullPath;
    }

    internal static async Task WriteNewFileAsync(string fullPath, string content, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("Destination file path must include a directory.", nameof(fullPath));
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                4096,
                FileOptions.Asynchronous))
            await using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                await writer.WriteAsync(content.AsMemory(), cancellationToken).ConfigureAwait(false);
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, fullPath, false);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            throw;
        }
    }
}
