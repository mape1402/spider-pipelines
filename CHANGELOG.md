# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

## [v2.1.0] - 2026-08-07

### Added

- `Spider.Testing` package for boundary, execution trace, ordering, transaction, and failure simulation tests.

## [v2.0.0] - 2026-07-24

### Added

- Explicit cancellation result state.
- Provider-agnostic execution boundaries wrapping full pipeline execution.
- Middleware support around target handlers.
- Concise pipeline builder shortcut methods.
- Benchmarked pipeline execution scenarios.
- .NET 8, .NET 9, and .NET 10 target framework support.
- `PipelineExecutionBoundary` base class with no-op boundary operations.

### Changed

- Parallel execution failures are captured consistently as pipeline failures.
- Parallel execution now has a single clear meaning: parallel steps run concurrently with the target.
- Override handlers without a condition now apply by default.
- Pipeline contracts for ordering, boundaries, errors, cancellation, and parallel execution are documented.
- Execution boundary configuration now lives on the bridge before `Attach`, including DI-resolved and delegate-based boundaries.

### Fixed

- Configuration APIs now validate null handlers and configuration delegates early.

## [v1.0.0] - 2025-07-18

### Added

- First stable release of Spider.Pipelines core.
