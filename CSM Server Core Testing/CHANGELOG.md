# CSM Server Testing CHANGELOG

## [5.0.0] - 27.07-2026

### Changes

- Renamed package from [CSM.Server.Core.Testing] to [CSM.Server.Testing].
- Changed support for correct entity service unit tests.
- Changed support for correct entity service integration tests. 

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| Moq				                      | 4.20.72          | 4.20.72         |
| xunit.v3			                      | 3.2.2            | 3.2.2           |
| CSM.Foundation.Core                     | -.-.-            | 4.0.0           |
| CSM.Database.Testing					  | -.-.-            | 4.0.0           |

| Projects                                | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Server.Core	                      | -.-.-            | 5.0.0           |

## [4.0.0] - 13.02-2026

### Changes

- Removed the method [Update_UpdatesEntity_Create] and [Update_UpdatesEntity_NotCreate] for [Update_UpdateFromInput] using [InlineData] technique.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| Moq				                      | -.-.-            | 4.20.72         |
| xunit.v3			                      | -.-.-            | 3.2.2           |

## [3.0.0] - 13.02-2026

### Changes

- Removed the method [Create_CreateMultipleEntities_Sync] and [Create_CreateMultipleEntities_NotSync] for [Create_BatchEntityCreation] using [InlineData] technique.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| Moq				                      | -.-.-            | 4.20.72         |
| xunit.v3			                      | -.-.-            | 3.2.2           |

## [1.0.0] - 05.02-2026

### Init

- Initialized package adding resources for a DB Creation using EF Core about security.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| Moq				                      | -.-.-            | 4.20.72         |
| xunit.v3			                      | -.-.-            | 3.2.2           |