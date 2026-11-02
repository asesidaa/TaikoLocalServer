# Phase 25 Context: White Runtime Capability Binding and Rewards

## Goal

Bind White identity, userdata, normal play, metadata readback, Dani where the White wire/data contract matches, and protocol-backed reward/Don Point state through White-owned persistence.

## Decisions

- **D-25-01:** White runtime state uses White-owned EF tables and DbSets. Shared AC15 helpers may remove duplicate algorithms, but no Blue, Green, Yellow, Red, or Nijiiro gameplay table is reused.
- **D-25-02:** White `playresult.php` supports normal and Dani mutation only. Tokkun, battle, ghost, WaiWai, item shop, Banacoin, and stateful Don Challenge remain absent unless later White evidence proves them.
- **D-25-03:** White reward state in Phase 25 is the protocol-backed `get_donpoint`, `reward_ptn`, `reward_progress`, and unlock flag surface already present in White playresult/userdata wire. Detailed `present.xml` and `spacialbaid.xml` provenance is Phase 26 collectable work.
- **D-25-04:** White `userdata.php` reads back ordinary AC15 arrays, favorites, recents, reward/Don Point values, and mode flags. The White wire has no Tokkun tutorial readback field, so the mapper intentionally ignores tutorial DTO values.
- **D-25-05:** White initial-data legal-term rows are disabled because the generated White initial-data response has no legal-term placement.
- **D-25-06:** White mappers remain Mapperly source-generator driven. Nontrivial White mappings are verified from generated `.g.cs` output after a temp-output Host build with `EmitCompilerGeneratedFiles=true`.

## Stop Rule

Manual RPCS3/cabinet and WebUI verification is reserved for Phase 27 closeout per user instruction. Phase 25 verification is automated build/test plus generated-source inspection only.
