# Changelog

## [3.0.0] - 2026-11-16
### Changed
- [Breaking Change] Task now takes fixed width flat file (provided as a string content or read from a file) as input instead of the `List<Dictionary<string, dynamic>>`.
- [Breaking Change] Flat file can now be given as a string or read from a file (`Input.SourceType`, `Input.FlatFilePath`), and the XML can be returned as a string or written to a new file (`Options.DestinationType`, `Options.DestinationFilePath`, `Result.FilePath`). An existing destination file is never overwritten.
- `HeaderRow`, `HeaderDelimiter` and `ColumnSpecifications` properties are added to `Options` tab to define the structure of the flat file.
- Added `SkipRows`, `SkipRowsFromTop` and `SkipRowsFromBottom` options.
- A flat file without any data rows (e.g. header only, or all rows skipped) now produces an XML with empty `Rows` element instead of an error.
- Values are parsed using the culture given in `Options.Culture` (invariant culture when empty), and the XML is always written using invariant culture, so the result no longer depends on the culture of the agent.

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
