using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using Frends.FixedWidthFlatFile.ConvertToJSON.Helpers;

namespace Frends.FixedWidthFlatFile.ConvertToJSON;

/// <summary>
/// Task's main class.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Task parses fixed width flat file content and converts it to a json string.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FixedWidthFlatFile.ConvertToJSON)
    /// </summary>
    /// <param name="input">Flat file content and its specifications.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">CancellationToken given by Frends.</param>
    /// <returns>object { bool Success, Error Error, string Data, string FilePath }</returns>
    public static Result ConvertToJSON(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);
            cancellationToken.ThrowIfCancellationRequested();

            var destinationPath = options.DestinationType == DataLocation.File
                ? FileHandler.ResolveDestinationPath(options.DestinationFilePath)
                : null;

            var content = input.SourceType == DataLocation.File
                ? FileHandler.ReadSource(input.FlatFilePath, cancellationToken)
                : input.FlatFileContent;

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Flat file content is empty.");

            var culture = string.IsNullOrWhiteSpace(options.Culture)
                ? CultureInfo.InvariantCulture
                : new CultureInfo(options.Culture, false);
            var data = FlatFileParser.Parse(content, options, culture, cancellationToken);
            var json = JsonBuilder.Build(data, cancellationToken);
            if (destinationPath == null)
                return new Result(true, json);

            FileHandler.WriteNewFile(destinationPath, json, cancellationToken);
            return new Result(true, filePath: destinationPath);
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}
