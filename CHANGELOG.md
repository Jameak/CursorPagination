# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) and this project adheres to [Semantic Versioning](http://semver.org/).

## [1.1.1] - 2026-01-02

Fixes:
- Fix last-page check when KeySet-paginating struct types by enforcing that the paginated type must be a class.

## [1.1.0] - 2025-12-13

New features:
- Add support for using nested properties in the pagination. These can be specified via plain strings or via _fullnameof_.
- Add additional warning diagnostics for suspicious nameof-usage.

## [1.0.0] - 2025-11-22

Initial release
