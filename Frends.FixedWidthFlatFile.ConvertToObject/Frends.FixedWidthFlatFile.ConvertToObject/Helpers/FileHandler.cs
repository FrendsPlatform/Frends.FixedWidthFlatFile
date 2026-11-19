namespace Frends.FixedWidthFlatFile.ConvertToObject.Helpers;

/// <summary>
/// Reads the source file.
/// </summary>
internal static class FileHandler
{
    internal static string ReadSource(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Flat file source '{path}' was not found.", path);

        return File.ReadAllText(path);
    }
}
