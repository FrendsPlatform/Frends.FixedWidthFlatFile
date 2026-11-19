using System.ComponentModel;
using Frends.FixedWidthFlatFile.ConvertToObject.Definitions;
using Frends.FixedWidthFlatFile.ConvertToObject.Helpers;

namespace Frends.FixedWidthFlatFile.ConvertToObject;

/// <summary>
/// Task Class for FixedWidthFlatFile operations.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Task to convert Fixed-width flat file to c# object
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-FixedWidthFlatFile-ConvertToObject)
    /// </summary>
    /// <param name="input">Flat file source.</param>
    /// <param name="options">Column, header and additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, List&lt;Dictionary&lt;string Key, dynamic Value&gt;&gt; Data, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result ConvertToObject(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);

            var content = input.SourceType == DataLocation.File
                ? FileHandler.ReadSource(input.FlatFilePath)
                : input.FlatFileContent;

            var outputData = FlatFileParser.Parse(content, options, cancellationToken);

            return new Result
            {
                Success = true,
                Data = outputData,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}
