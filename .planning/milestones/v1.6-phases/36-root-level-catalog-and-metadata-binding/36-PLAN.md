---
status: ready
phase: 36
title: Root-Level Catalog and Metadata Binding
---

# Phase 36 Plan

## Goal

Load KIMIDORI root-level catalog data and serve only supported metadata routes.

## Tasks

1. Add KIMIDORI catalog interfaces, required-file checks, and root-level game-data paths.
2. Bind KIMIDORI catalog loading to `Host/wwwroot/data/kimidori/data`.
3. Add intentional KIMIDORI sidecar JSON for implemented server-authored metadata.
4. Add KIMIDORI metadata handlers and controller mappings for supported route families.
5. Keep Taikojuku sidecar loading and practice-folder behavior absent.

## Verification

- Catalog loader tests for root-level files.
- Metadata route/handler tests for catalog-backed or conservative no-state responses.
- Host output copy check for KIMIDORI sidecars.
