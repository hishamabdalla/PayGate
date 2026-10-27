# Changelog

All notable changes to this project are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [Semantic Versioning](https://semver.org/). All packages share one version.

## [Unreleased]

### Added
- Repository foundation: solution, strict build settings, central package management, public API tracking, versioning from git tags, CI, release and nightly sandbox workflows.
- Empty package shells: `PayGate.Abstractions`, `PayGate`, `PayGate.AspNetCore`, `PayGate.Kashier`, `PayGate.Testing`.
- `PayGate.Abstractions`: provider and account identity, `Money`/`Currency`, payment creation, lookup, refund and cancel models, normalized status with transition rules, `PaymentResult<T>`/`PaymentError`, capabilities, gateway contracts, and webhook/event/return-callback models.
