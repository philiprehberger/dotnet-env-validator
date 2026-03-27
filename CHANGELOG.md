# Changelog

## 0.2.8 (2026-03-26)

- Add Sponsor badge to README
- Fix License section format

## 0.2.7 (2026-03-24)

- Add unit tests
- Add test step to CI workflow

## 0.2.6 (2026-03-22)

- Add dates to changelog entries

## 0.2.5 (2026-03-21)

- Align csproj description with README

## 0.2.4 (2026-03-20)

- Add API section to README
- Add LangVersion and TreatWarningsAsErrors to csproj

## 0.2.3 (2026-03-16)

- Add Development section to README
- Add GenerateDocumentationFile and RepositoryType to .csproj

## 0.2.0 (2026-03-13)

- Add enum type support with case-insensitive parsing
- Add `Pattern` property for regex validation of environment variable values
- Add collection parsing for `string[]`, `int[]`, `List<string>`, and `List<int>` properties
- Add `Separator` property for custom collection delimiters (default comma)

## 0.1.1 (2026-03-10)

- Add README to NuGet package so it displays on nuget.org

## 0.1.0 (2026-03-09)

- Initial release
- `[EnvVar]` attribute for declarative environment variable binding
- `EnvValidator.Validate<T>()` with automatic type conversion
- Support for required/optional, defaults, and choices
- Built-in type coercion: string, int, long, double, bool, Uri, TimeSpan
