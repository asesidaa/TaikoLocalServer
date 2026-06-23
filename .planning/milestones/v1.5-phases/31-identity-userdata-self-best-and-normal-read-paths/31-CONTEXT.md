# Phase 31: Identity, Userdata, Self-Best, And Normal Read Paths - Context

**Gathered:** 2026-06-21
**Status:** Ready for planning

## Boundary

Expose Murasaki-owned identity, userdata, self-best, crown, favorite, recent-song, release-song, recommendation, folder/telop, and Taikojuku read paths before special Murasaki surfaces.

## Decisions

- Murasaki uses separate save, score, favorite, recent, and Dani tables.
- Shared identity remains shared only at card/user/credential level.
- `bestscore.php` is not used as a substitute for per-user self-best readback.
