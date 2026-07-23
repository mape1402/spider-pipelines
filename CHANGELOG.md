# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- Explicit cancellation result state.
- Middleware support around target handlers.
- Concise pipeline builder shortcut methods.
- Benchmarked pipeline execution scenarios.

### Changed

- Parallel execution failures are captured consistently as pipeline failures.
- Parallel execution now has a single clear meaning: parallel steps run concurrently with the target.
- Override handlers without a condition now apply by default.
- Pipeline contracts for ordering, errors, cancellation, and parallel modes are documented.

### Fixed

- Configuration APIs now validate null handlers and configuration delegates early.

## [v1.0.0] - 2025-07-18

### Added

- First stable release of Spider.Pipelines core.
