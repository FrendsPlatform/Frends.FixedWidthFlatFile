using System.ComponentModel;
using Frends.FixedWidthFlatFile.CreateFromObject.Definitions;
using Frends.FixedWidthFlatFile.CreateFromObject.Helpers;

namespace Frends.FixedWidthFlatFile.CreateFromObject;

/// <summary>
/// Task Class for FixedWidthFlatFile operations.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Converts rows of objects into a fixed-width flat file.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-FixedWidthFlatFile-CreateFromObject)
    /// </summary>
    /// <param name="input">Data rows to format.</param>
    /// <param name="options">Column layout, header settings, output destination and additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string FlatFileContent, string FilePath, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> CreateFromObject(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);
            cancellationToken.ThrowIfCancellationRequested();

            if (options.ColumnSpecifications.Length == 0)
            {
                throw new ArgumentException("At least one column specification is required.", nameof(options));
            }

            if (!Enum.IsDefined(options.HeaderRow))
            {
                throw new ArgumentOutOfRangeException(nameof(options.HeaderRow), "Unsupported header row format.");
            }

            var columnNames = FlatFileParser.GetColumnNames(options.ColumnSpecifications);
            var rows = new List<string>();
            switch (options.HeaderRow)
            {
                case HeaderRowType.FixedWidth:
                    rows.Add(FlatFileParser.FormatHeader(columnNames, options.ColumnSpecifications));
                    break;
                case HeaderRowType.Delimited when string.IsNullOrEmpty(options.HeaderDelimiter) ||
                                                  options.HeaderDelimiter.Length != 1:
                    throw new ArgumentException(
                        "Header delimiter must contain exactly one character.",
                        nameof(options));
                case HeaderRowType.Delimited:
                    rows.Add(string.Join(options.HeaderDelimiter, columnNames));
                    break;
                case HeaderRowType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var startIndex = 0;
            var endIndex = input.Data.Count;
            if (options.SkipRows)
            {
                startIndex = Math.Min(Math.Max(options.SkipRowsFromTop, 0), input.Data.Count);
                var remainingCount = input.Data.Count - startIndex;
                endIndex -= Math.Min(Math.Max(options.SkipRowsFromBottom, 0), remainingCount);
            }

            for (var rowIndex = startIndex; rowIndex < endIndex; rowIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dataRow = input.Data[rowIndex];
                if (dataRow == null)
                {
                    throw new ArgumentException("Data rows cannot contain null values.", nameof(input));
                }

                var values = new object[columnNames.Count];
                for (var index = 0; index < columnNames.Count; index++)
                {
                    if (!dataRow.TryGetValue(columnNames[index], out values[index]))
                    {
                        throw new ArgumentException(
                            $"Data row is missing the '{columnNames[index]}' column.",
                            nameof(input));
                    }
                }

                rows.Add(FlatFileParser.FormatRow(values, options.ColumnSpecifications));
            }

            var flatFileContent = string.Join(Environment.NewLine, rows);
            if (options.DestinationType != DataLocation.File)
            {
                return new Result
                {
                    Success = true,
                    FlatFileContent = flatFileContent,
                    FilePath = null,
                    Error = null,
                };
            }

            var destinationPath = FileHandler.ResolveDestinationPath(options.DestinationFilePath);
            await FileHandler.WriteNewFileAsync(destinationPath, flatFileContent, cancellationToken)
                .ConfigureAwait(false);
            return new Result
            {
                Success = true,
                FlatFileContent = null,
                FilePath = destinationPath,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}
