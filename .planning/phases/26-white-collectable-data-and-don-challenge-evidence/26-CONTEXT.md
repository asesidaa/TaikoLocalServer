# Phase 26 Context: White Collectable Data and Don Challenge Evidence

## Goal

Collect White 0.13 collectable data with local provenance, bind data that has a compatible raw-data contract, and decide whether White Don Challenge can be stateful.

## Decisions

- **D-26-01:** White `present.xml` is local runtime evidence for Don Point reward rows. The server keeps the raw item type code numeric because Phase 26 proves row structure, not safe semantic names for each type value.
- **D-26-02:** White `spacialbaid.xml` is local runtime evidence for special BAID rows. The server exposes parsed BAID/access-code/comment facts through the White catalog but does not mutate identity behavior from them.
- **D-26-03:** White Don Challenge remains absent/data-only. White proto has embedded userdata/playresult challenge fields, but current evidence does not prove a standalone route, active bundle data, reward timing, readback mutation, or state semantics.
- **D-26-04:** Red standalone Don Challenge routes, Red progress tables, and Red reward evaluator must not be copied to White without White route/data/runtime evidence.

## Stop Rule

Manual RPCS3/cabinet and WebUI verification is reserved for Phase 27 closeout per user instruction. Phase 26 verification is automated build/test only.
