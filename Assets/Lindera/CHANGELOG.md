# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- CI/CD support with GitHub Actions
  - Automated Unity tests (EditMode/PlayMode)
  - Automated native library builds for all platforms
  - Automated UPM release on tag creation
- Pre-built native libraries for all supported platforms
  - macOS Universal (x64 + ARM64)
  - Linux x64
  - iOS ARM64 (static library)
  - Android ARM64 and ARMv7
- Platform-specific Unity meta files with correct import settings
- Native library build documentation in README

## [0.2.0] - 2025-01-13

### Changed
- Sample UI migrated from OnGUI to UGUI with TextMeshPro
- Sample scene now uses Input System package for modern input handling

### Added
- Noto Sans CJK JP font for Japanese text rendering in sample
- Dynamic TMP Font Asset generation in Editor setup
- Automatic UI setup via Lindera menu

### Fixed
- Japanese text rendering (文字化け) in sample scene

## [0.1.0] - 2025-01-13

### Added
- Initial release
- Japanese text tokenization using Lindera 2.0
- Reading (furigana) extraction from IPADIC dictionary
- Part-of-speech tagging support
- Multi-platform native library support (Windows x64)
- IDisposable pattern for proper resource cleanup
- UniTask integration for async operations
- Editor and Runtime tests
- Sample scene with basic usage example
