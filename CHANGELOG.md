# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

### [1.4.4](https://github.com/kalulas/TableCraft/compare/v1.4.3...v1.4.4) (2025-08-17)


### Features

* **editor:** move perforce related configuration outside of appsettings.json ([be89838](https://github.com/kalulas/TableCraft/commit/be898385834147a5eca198e9328f3890e3c0c3ce))


### Bug Fixes

* add missing PerforceUserConfig.cs with JsonIgnore attribute ([7b125f3](https://github.com/kalulas/TableCraft/commit/7b125f3001218f33e7bdcae081f9aa23f5032236))

### [1.4.3](https://github.com/kalulas/TableCraft/compare/v1.4.2...v1.4.3) (2025-08-10)

### [1.4.2](https://github.com/kalulas/TableCraft/compare/v1.4.0...v1.4.2) (2025-08-03)


### Bug Fixes

* **editor:** ensure logging works when appsettings.json is invalid ([7de8b7a](https://github.com/kalulas/TableCraft/commit/7de8b7af53bc67978c9cf8e6cc113d46f132b304))

## [1.4.0](https://github.com/kalulas/TableCraft/compare/v1.3.1...v1.4.0) (2025-08-03)

### [1.3.1](https://github.com/kalulas/TableCraft/compare/v1.3.0...v1.3.1) (2024-01-13)


### Bug Fixes

* **editor:** fix the "About" button's enable state ([a9788cc](https://github.com/kalulas/TableCraft/commit/a9788cc8b86ff918316393a6d99fb2e13a36aea1))

## [1.3.0](https://github.com/kalulas/TableCraft/compare/v1.2.0...v1.3.0) (2024-01-13)


### Features

* **editor:** add a fuzzy search box for table list ([8a50e84](https://github.com/kalulas/TableCraft/commit/8a50e84aad94840271d0c0bf57908c46078bf226))


### Bug Fixes

* **editor:** remove a absolute reference path in .csproj ([6dad365](https://github.com/kalulas/TableCraft/commit/6dad3656cb84fb9c46d334af7f23d7be781f9f31))

## [1.2.0](https://github.com/kalulas/TableCraft/compare/v1.1.0...v1.2.0) (2023-06-17)


### Features

* **core:** provide new Generator's API to get/set the output file path(not recommended) ([0398c7b](https://github.com/kalulas/TableCraft/commit/0398c7bfbe2c8ed196c2b3e611978b04322d3b33))
* **core:** usage group configuration / export support ([d4cf515](https://github.com/kalulas/TableCraft/commit/d4cf515363ab7dc861a6d2ccf014dfbb7c288db6))
* **editor:** usage group information / export support ([88e622d](https://github.com/kalulas/TableCraft/commit/88e622d2297cffc7096b42845e1db776ee8b19dd))


### Bug Fixes

* **core/editor:** usage group export changes ([543f89d](https://github.com/kalulas/TableCraft/commit/543f89d76b35bcf36cd2ddea80b7035c1a2eed55))
* **core:** optimize error log / change usage group to optional ([71decdd](https://github.com/kalulas/TableCraft/commit/71decdd8bf938bc6787c31dde7a831c9db0072a8))

## [1.1.0](https://github.com/kalulas/ConfigCodeGen/compare/v1.0.1...v1.1.0) (2023-05-19)


### Features

* **editor:** add a save-on-generate toggle above button group ([d754beb](https://github.com/kalulas/ConfigCodeGen/commit/d754beb04b4063e6879be37b227f77c7d54bf05d))
* **editor:** auto select the first collection type on select value type ([35871e5](https://github.com/kalulas/ConfigCodeGen/commit/35871e560b6322ccdefcad471c1cd14ebffa9dfd))

### [1.0.1](https://github.com/kalulas/ConfigCodeGen/compare/v1.0.0...v1.0.1) (2023-05-19)


### Bug Fixes

* **core:** fix the NullReferenceException if ConfigJson contains illegal usage ([5caf48a](https://github.com/kalulas/ConfigCodeGen/commit/5caf48ab64e2c587448041ec15918bb1973d5799))
* **editor:** handle UnauthorizedAccessException on UpdateVersionControlConfig ([94c74e2](https://github.com/kalulas/ConfigCodeGen/commit/94c74e22350316dc8a41ee2bded52694dea59d63))

## [1.0.0](https://github.com/Kalulas/TableCraft/compare/v0.4.0-alpha...v1.0.0) (2023-05-15)


### ⚠ BREAKING CHANGES

* "ConfigReader" namespace no longer available

### Features

* **core/editor:** support format string for generated output ([c477edd](https://github.com/Kalulas/TableCraft/commit/c477edd39d6b3368bae563f3901fdcda09b5d4fb))
* **core:** adjust perforce BeforeWrite / AfterWrite strategy ([ed3b151](https://github.com/Kalulas/TableCraft/commit/ed3b151e88eb881e9f966a7ac9746f3cb87dabd1))
* **editor:** open a markdown MessageBox on 'About' button clicked ([f771959](https://github.com/Kalulas/TableCraft/commit/f77195994c2aaa73c3fc2459738fc128943bdb2e))


### Bug Fixes

* **core:** fix a null reference exception in BeforeWrite(), AfterWrite() ([d794942](https://github.com/Kalulas/TableCraft/commit/d794942a855efe56969e9313eeafab929a00cfe4))
* **core:** remove sensitive content(password) from log ([14fbcc2](https://github.com/Kalulas/TableCraft/commit/14fbcc2df934829dec7661303f52d5835382bd7d))
* **core:** remove unused field 'm_FileExisted' ([5157028](https://github.com/Kalulas/TableCraft/commit/51570282e04a2286b0ebabf0a3cf00d2f7543a7b))
* **perforce:** run Disconnect() in another coroutine to avoid blocking the main thread ([1699dcc](https://github.com/Kalulas/TableCraft/commit/1699dccaf9263579e7d00ee0e83f3fe023a0c610))
* **readme:** fix indent problem, add a file link in .md ([d190822](https://github.com/Kalulas/TableCraft/commit/d190822c9041f4a5f53872ac5c4949193683fd21))


* rename ConfigReader to ConfigElements ([ac1f700](https://github.com/Kalulas/TableCraft/commit/ac1f700419dd30ccfc3924e03a9220d92d20fef7))
