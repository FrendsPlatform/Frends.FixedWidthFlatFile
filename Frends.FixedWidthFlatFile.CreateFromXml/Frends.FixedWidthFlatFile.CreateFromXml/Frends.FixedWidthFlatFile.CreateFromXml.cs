using System.ComponentModel;
using Frends.FixedWidthFlatFile.CreateFromXml.Definitions;
using Frends.FixedWidthFlatFile.CreateFromXml.Helpers;

namespace Frends.FixedWidthFlatFile.CreateFromXml;

/// <summary>
/// Task Class for FixedWidthFlatFile operations.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Creates a fixed width flat file from XML (Root/Rows/Row/{column}). The opposite of ConvertToXML task.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-FixedWidthFlatFile-CreateFromXml)
    /// </summary>
    /// <param name="input">XML content or path.</param>
    /// <param name="options">Flat file structure and additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Data, string FilePath, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result CreateFromXml(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);

            var destinationPath = options.DestinationType == DataLocation.File
                ? FileHandler.ResolveDestinationPath(options.DestinationFilePath)
                : null;

            using TextReader reader = input.SourceType == DataLocation.File
                ? FileHandler.OpenSourceFile(input.XmlFilePath)
                : new StringReader(input.XmlContent);

            var document = FlatFileWriter.ReadXml(reader);
            cancellationToken.ThrowIfCancellationRequested();

            var flatFile = FlatFileWriter.Write(document, options, cancellationToken);

            if (destinationPath == null)
                return new Result { Success = true, Data = flatFile };

            FileHandler.WriteNewFile(destinationPath, flatFile);
            return new Result { Success = true, FilePath = destinationPath };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}
