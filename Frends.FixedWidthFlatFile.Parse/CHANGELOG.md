# Changelog

## [2.0.0] - 2026-09-10
### Changed
- Target framework updated to .NET 8.
- Task now returns `Success` and `Error` properties in the result, and accepts a cancellation token so that a running Task can be canceled.
- Added `ThrowErrorOnFailure` and `ErrorMessageOnFailure` options to control how failures are reported. By default, errors are thrown as before.
### Fixed
- Parsed row values are no longer exposed as `dynamic` values in the result data.

## [1.0.2] - 2023-08-08
### Changed
- Documentational changes.

## [1.0.1] - 2023-06-13
### Changed
- Documentation updates.

## [1.0.0] - 2022-03-24
### Added
- Initial implementation