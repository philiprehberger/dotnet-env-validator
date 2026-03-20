# Changelog

## 0.2.4

- Add API section to README
- Add LangVersion and TreatWarningsAsErrors to csproj

## 0.2.3

- Add Development section to README
- Add GenerateDocumentationFile and RepositoryType to .csproj

## 0.2.0 (2026-03-13)

### Added
- Enum type support with case-insensitive parsing
- `Pattern` property for regex validation of environment variable values
- Collection parsing for `string[]`, `int[]`, `List<string>`, and `List<int>` properties
- `Separator` property for custom collection delimiters (default comma)

## 0.1.1 (2026-03-10)

- Add README to NuGet package so it displays on nuget.org

## 0.1.0 (2026-03-09)

- Initial release
- `[EnvVar]` attribute for declarative environment variable binding
- `EnvValidator.Validate<T>()` with automatic type conversion
- Support for required/optional, defaults, and choices
- Built-in type coercion: string, int, long, double, bool, Uri, TimeSpan
