# Changelog

## [3.0.0] - 2026-11-16
### Changed
- [Breaking Change] Task now takes fixed width flat file content as input instead of the `List<Dictionary<string, dynamic>>` produced by `Frends.FixedWidthFlatFile.Parse`, so the Parse Task is no longer needed. `Input.FileContent` is replaced with `FlatFileContent`, `HeaderRow`, `HeaderDelimiter` and `ColumnSpecifications`.
- [Breaking Change] Added `SkipRows`, `SkipRowsFromTop` and `SkipRowsFromBottom` options, which work the same way as in the Parse Task.
- A flat file without any data rows (e.g. header only, or all rows skipped) now produces an XML with empty `Rows` element instead of an error.
- Values are formatted using invariant culture, so the result no longer depends on the culture of the agent.
- Cancellation is no longer converted to a generic exception.

## [2.0.0] - 2026-09-10
### Changed
- [Breaking Change] Renamed the `data` parameter to `input`.
- Added `Options` parameter with `ThrowErrorOnFailure` and `ErrorMessageOnFailure` properties, allowing errors to be returned as a failed result instead of throwing an exception.
- `Result` object now includes `Success` and `Error` properties in addition to `Data`.
- Target framework changed to net8.0.

## [1.1.0] - 2026-04-16
### Updated
- Updated Result.Data documentation and task description.

## [1.0.1] - 2023-07-26
### Updated
- Updated task documentation.

## [1.0.0] - 2022-04-07
### Added
- Initial implementation
