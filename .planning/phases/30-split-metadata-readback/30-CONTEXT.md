# Phase 30: Split Metadata Readback - Context

**Gathered:** 2026-06-21
**Status:** Ready for planning

## Boundary

Implement proven Murasaki split metadata routes and operational no-state routes without adding White-style `initialdatacheck.php`.

## Decisions

- `defaultsong.php`, `mainichisong.php`, `foldercheck.php`, `getfolder.php`, `telopcheck.php`, and `gettelop.php` are backed by application queries and Murasaki mappers/controllers.
- `heartbeat.php` and `bookkeeping.php` are no-state compatibility endpoints.
- Unknown byte-heavy or authority-heavy surfaces stay out of scope until Phase 33 evidence.
