# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

## [v2.2.0] - 2026-10-04

### Added

- Compile-time architecture manifest source generator for configured pipelines and composed flows.
- Manifest descriptors for pipeline stages, boundaries, flow steps, flow conditions, flow branches, flow profiles, relations, and source evidence.
- Descriptive metadata APIs with `Named`, `Describe`, and `Tags` for flows, branch routes, pipelines, pipeline stages, and boundaries.
- Runtime tracing with opt-in registration, bounded dispatch, in-memory storage by default, live stream hooks, sinks, observers, and replaceable trace stores.
- `Spider.Pipelines.Web` package for graphical architecture documentation from generated manifests.
- Web runtime trace views with execution story lines, flow graphs, raw event inspection, trace import, and trace export.
- Web sample project that serves the graphical Spider architecture documentation UI and runtime trace examples.

### Changed

- Web architecture documentation now highlights related flows, pipelines, boundaries, and parent relationships with clearer navigation.
- Flow and pipeline diagrams now use distinct visual shapes for decisions, routes, parallel work, batches, linked flows, stages, and boundaries.
- The documentation shell now supports light and dark modes, a collapsible sidebar, maximized graph views, and refined navigation iconography.

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
