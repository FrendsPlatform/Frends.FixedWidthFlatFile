using System.ComponentModel;
using System.Xml.Linq;
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
            cancellationToken.ThrowIfCancellationRequested();
            ValidationHandler.Run(input, options);

            var destinationPath = options.DestinationType == DataLocation.File
                ? FileHandler.ResolveDestinationPath(options.DestinationFilePath)
                : null;

            XDocument document;
            if (input.SourceType == DataLocation.File)
            {
                // Reading from a stream lets the XML parser detect the encoding from the BOM or XML declaration.
                using var stream = FileHandler.OpenSourceFile(input.XmlFilePath);
                document = FlatFileWriter.ReadXml(stream);
            }
            else
            {
                using var reader = new StringReader(input.XmlContent);
                document = FlatFileWriter.ReadXml(reader);
            }

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
