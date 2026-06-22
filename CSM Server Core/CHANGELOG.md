# CSM Server Core CHANGELOG

## [2.2.0] - 22.06-2026

### Changes

- Updated packages [CSM.Database.Core] and [CSM.Foundation.Core].

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Database.Core                       | -.-.-            | 6.0.6           |
| CSM.Foundation.Core                     | 2.2.3            | 4.0.0           |

## [2.1.9] - 13.02-2026

### Patched

- Parched [CSM.Foundation.Core] package to get an important fix for error handling.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.1            | 2.2.3           |

## [2.0.0] - 25.12-2025

### Changed

- Now Start engine method handles automatic configurations and middlewares.

### Added

- Now start engine method requires the framing middleware instance configuration to work.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.1            | 2.1.1           |

## [1.1.0] - 25.12-2025

### Changed

- Now [ServerUtils] start method checks if the solution has a server module to use as a configuration.

### Added

- A new interface [IServerModule] was added to identify a customization server module to inject configurations.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.1            | 2.1.1           |

## [1.0.0] - 24.12-2025

### Init

- Initialized package adding resources for a DB Creation using EF Core about security.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | -.-.-            | 2.1.1           |