# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

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
