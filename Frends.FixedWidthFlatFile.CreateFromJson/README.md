# Frends.FixedWidthFlatFile.CreateFromJson

Converts JSON data to a fixed-width flat file.

The input must be a non-empty JSON array of objects. Configure columns in output order with a property name, type, and field width. Values are left-aligned and padded with spaces; values wider than their configured field fail rather than being truncated. Missing or null properties become blank fields, and properties not listed in the column definitions are ignored. An optional header row uses the column names.

Example JSON:

```json
[
  { "Name": "Veijo", "StartDate": "2018-05-27T00:00:00" }
]
```

With `Name` configured as a string of width 8 and `StartDate` configured as a date using `yyyyMMdd` and width 8, the output record is `Veijo   20180527`.

[![Build status](https://github.com/FrendsPlatform/Frends.FixedWidthFlatFile/actions/workflows/CreateFromJson_main_release.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.FixedWidthFlatFile/actions/workflows/CreateFromJson_main_release.yml)
![Code Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.FixedWidthFlatFile/Frends.FixedWidthFlatFile.CreateFromJson|main)
[![Repository License](https://img.shields.io/github/license/FrendsPlatform/Frends.FixedWidthFlatFile?label=License)](https://github.com/FrendsPlatform/Frends.FixedWidthFlatFile/blob/main/LICENSE)

## Installing

You can install the Task via Frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.FixedWidthFlatFile.git`

### Build the project

`dotnet build`

### Run tests

Run the tests

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`

### StyleCop.Analyzers Version
This project uses StyleCop.Analyzers 1.2.0-beta.556, as recommended by the author, to get the latest fixes and improvements not available in the last stable release.
