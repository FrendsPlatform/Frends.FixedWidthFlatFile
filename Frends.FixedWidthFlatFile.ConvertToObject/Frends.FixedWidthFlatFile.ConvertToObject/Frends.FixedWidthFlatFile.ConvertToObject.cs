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
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    // TODO: Remove Connection parameter if the task does not make connections
    public static Result ConvertToObject(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection, options);

            // Cancellation token should be provided to methods that support it
            // and checked during long-running operations, e.g., loops
            cancellationToken.ThrowIfCancellationRequested();

            if (input.Repeat < 0)
                throw new Exception("Repeat count cannot be negative.");

            var output = string.Join(options.Delimiter, Enumerable.Repeat(input.Content, input.Repeat));

            return new Result
            {
                Success = true,
                Output = output,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}
