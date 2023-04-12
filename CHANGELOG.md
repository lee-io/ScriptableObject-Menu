# Changelog
All notable changes to Scriptable Object Menu will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2023-04-12

### Added
- Custom Editor for the Project Settings window

### Changed
- Moved the settings file from the Assets folder to the Project Settings folder

### Fixed
- Performance regression in large projects with 'Include Selected Types' enabled

## [2.0.0] - 2023-01-29

### Added
- Shortcut to create assets from selected types in the Project Window
- Shortcut to clone assets from selected types in the Project Window
- Ability to open newly created scripts with associated application
- Ability to sort asset menu items alpha-numerically
- Ability to group asset menu items by assembly
- Editor only functions to the default template file
- Configurable properties to the Project Settings window
- Full support for user defined assemblies

### Changed
- Optimised type discovery via TypeCache (for v19.2+ users)
- Refactored for full integration with the Project Window

### Removed
- Access modifiers from the default template file

## [1.3.0] - 2021-12-20

### Added
- Unity package support
- Assembly filtering
- Dialog path synchronisation with the Project Window

## [1.0.0] - 2019-12-24
- Initial commit