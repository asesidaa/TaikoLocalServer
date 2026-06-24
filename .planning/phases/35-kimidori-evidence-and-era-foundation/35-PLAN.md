---
status: ready
phase: 35
title: KIMIDORI Evidence and Era Foundation
---

# Phase 35 Plan

## Goal

Create the KIMIDORI first-class era foundation with evidence-backed route/proto boundaries and no unsupported state.

## Tasks

1. Record KIMIDORI evidence from `proto/kimidori`, `.tools/kimidori/EBOOT.ELF.i64`, and linked data layout.
2. Generate adapter-local KIMIDORI wire DTOs from existing proto inputs without editing `proto/`.
3. Add `GameEra.Kimidori`, Host settings, adapter project registration, application-part gating, and `/v05r00/chassis/*` controllers.
4. Keep controllers separated from Murasaki even where scaffolded behavior is equivalent.
5. Add disabled-era and route regression coverage proving KIMIDORI does not alter existing eras.

## Verification

- Focused KIMIDORI protocol/route tests.
- Host build using temp output if needed.
- Evidence artifact documents no Taikojuku/Tokkun/Banacoin/battle route support.
