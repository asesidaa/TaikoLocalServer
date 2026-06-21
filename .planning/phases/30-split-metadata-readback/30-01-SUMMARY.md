# Phase 30 Summary: Split Metadata Readback

## Completed

- Added split metadata controllers as separate files.
- Mapped default-song and mainichi-song responses from common Murasaki catalog data.
- Mapped folder/telop readiness and data readback through Murasaki-specific mapper/controller shapes.
- Added no-state `heartbeat.php` and `bookkeeping.php`.
- Kept `initialdatacheck.php` absent for Murasaki.

## Deferred

- `songhash.php`, `bestscore.php`, `shoppingresult.php`, challenge arrays, and unknown byte payloads remain Phase 33.
