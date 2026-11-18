# Changelog

## [3.0.0] - 2026-11-18

### Changed

- **[Breaking Change]** The Task now takes fixed width flat file content directly and parses it internally.
- **[Breaking Change]** Flat file can be given as a string or read from a file (`Input.SourceType`,
  `Input.FlatFilePath`), and the json can be returned as a string or written to a new file (`Options.DestinationType`,
  `Options.DestinationFilePath`, `Result.FilePath`). An existing destination file is never overwritten.
- **[Breaking Change]** `Input.FileContent` replaced with `FlatFileContent`;
- **[Breaking Change]** `HeaderRow`, `HeaderDelimiter`, `ColumnSpecifications` and `Culture` are moved from `Input` to
  the `Options` tab.
- Added `SkipRows`, `SkipRowsFromTop` and `SkipRowsFromBottom` to `Options`.
- **[Breaking Change]** Values are parsed using the culture given in `Options.Culture` (invariant culture when empty). The
  previous guess of `fi-FI` for decimal and double values containing a comma was removed: set `Culture` (e.g. `fi-FI`)
  to parse such values. The json is always written using invariant culture, so the result no longer depends on the
  culture of the agent.

## [2.0.0] - 2026-09-10

### Changed

- [Breaking Change] Renamed the `data` parameter to `input` for consistency with other Frends Tasks.
- Added `Options` parameter with `ThrowErrorOnFailure` and `ErrorMessageOnFailure` properties to control error handling
  behavior.
- `Result` now includes `Success` and `Error` properties in addition to `Data`.
- Target framework updated to net8.0.

## [1.0.1] - 2023-07-26

### Added

- Updated documentationof the parameters and modified the Task's documentation.

## [1.0.0] - 2022-04-07

### Added

- Initial implementation
