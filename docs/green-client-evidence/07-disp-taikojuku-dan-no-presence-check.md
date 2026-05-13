# `disp_taikojuku_dan` — Client Reads Without Proto2 Presence Check

## Summary

The Green client's UserDataResponse consumer reads the `disp_taikojuku_dan_`
field directly from message memory at offset `+0x31C` without calling the
proto2-generated `has_disp_taikojuku_dan()` accessor. As a result, an absent
wire tag (which leaves the C++ class member at its constructor-init value of
`0`) is observationally identical to an explicit `=0` at the consumer site.

Both inputs reach `Taikojuku_GetDanSlotSongRange @ 0x127F98`, which then
indexes `g_TaikojukuDanSlotTable @ 0x1108CEC` as `table + 84 * dan - 84`.
For `dan = 0` this underflows the table and the caller's iteration loop walks
into unmapped memory, crashing the game on new-card boot.

The server-side fix is to always send a value in `[1, 25]`. The handler and
mapper now emit sentinel `1` for any out-of-range save value, including the
new-card default of `0`. `1` is the value the client itself uses as the
"no data" default in `sub_7FDFFC`.

## Reproduction

- Empty save (new card) on `feat/green-version-support` prior to this fix.
- `userdata.php` round-trips with `disp_taikojuku_dan` omitted on the wire.
- Client boots the song-list filter inside `sub_7FDFFC`, calls
  `Taikojuku_GetDanSlotSongRange(0)`, then iterates a bogus range.
- RPCS3 reports: `Access violation reading location 0x1A90000` at
  `CIA = 0x007FEDFC`, with the call stack `0x7FEDFC <- 0x1F7C58 <- 0x26DB34
  <- 0x8D2824 <- 0x619790 <- 0x2718FC <- ...`.

## Evidence

### Indexer and table

`Taikojuku_GetDanSlotSongRange @ 0x127F98`:

```c
__int64 __fastcall Taikojuku_GetDanSlotSongRange(__int64 result, __int64 a2)
{
  char *v2;

  result = (unsigned int)result;
  v2 = (char *)&g_TaikojukuDanSlotTable + 84 * a2 - 84;     // begin
  *(_DWORD *)(unsigned int)result = (_DWORD)v2;
  *(_DWORD *)((unsigned int)result + 4LL) =
      (_DWORD)v2 + 8 * *(_DWORD *)((unsigned int)v2 + 0x50LL);  // end
  return result;
}
```

Each slot row is 84 bytes; the song count is the `u32` at `+0x50` within
the row, and the end pointer is `begin + 8 * count`. For `dan = 0`:
- `begin = table - 84 = 0x1108CEC - 0x54 = 0x1108C98`
- `*(begin + 0x50) = *(0x1108CE8)` — four bytes immediately before the
  table; whatever the linker placed there.
- `end = 0x1108C98 + 8 * garbage`. If `garbage ≈ 0x352000`, `end ≈
  0x1108C98 + 0x1A90000`, which is well past the segment boundary.

### Three independent call sites read `*(msg + 0x31C)` with no presence check

**`sub_19CFE0:151`** (direct caller of the indexer):

```c
Taikojuku_GetDanSlotSongRange(&v44,
    *(unsigned int *)(*(unsigned int *)(v23 + 40) + 0x31CLL));
```

**`sub_1016F8:506`** (direct caller of the indexer):

```c
Taikojuku_GetDanSlotSongRange(&v111,
    *(unsigned int *)(*(unsigned int *)(v73 + 44) + 0x31CLL));
```

**`sub_24377C:176`** (calls via the `sub_A1BA7C` trampoline, which forwards
to `Taikojuku_GetDanSlotSongRange`):

```c
sub_A1BA7C(&v48,
    *(unsigned int *)(*(unsigned int *)(v27 + 40) + 0x31CLL));
```

All three load a pointer at parent-offset `+40` or `+44` (the embedded
UserDataResponse message pointer), then read `*(message + 0x31C)` —
the `disp_taikojuku_dan_` member — and pass it straight to the indexer.
None of them branches on `has_*()`, on a `_has_bits_` mask, or on the
value being non-zero.

### Crash site shows the client's own default is `1`

**`sub_7FDFFC:747-756`** (the crashing function, RPCS3 CIA `0x7FEDFC` is
inside its iteration loop):

```c
v105 = 1;                              // baked-in "no data" default
...
v47 = *(_BYTE *)(sub_A1B99C() + 16);   // some game-state byte
...
if ( !v47 )
  v105 = *(_DWORD *)(*(unsigned int *)(48 * v97 + j_4[2] + 0x28LL) + 0x31CLL);
sub_A1BA7C(&m_1, v105);                // -> Taikojuku_GetDanSlotSongRange
```

The client explicitly initialises its local slot to `1` and only overrides
it with the wire value on the `!v47` branch. This is the strongest
available signal that `1` is the safe sentinel: the client developer
encoded the same fallback we now mirror server-side.

### `green.proto` is `proto2`

```proto
syntax = "proto2";
...
optional uint32 disp_taikojuku_dan = 34;
```

`optional` in proto2 does cause codegen to emit `has_disp_taikojuku_dan()`
and the `_has_bits_` bitfield. But proto2 only *exposes* presence; it does
not *enforce* its use. The Green client's app code does not consult
`has_*()` for this field, so wire-absence is indistinguishable from
explicit `0` at the consumer.

## Server-side fix

- `Application/Handlers/UserDataQuery.Green.cs` — `GetSafeTaikojukuDanSlot`
  returns `1u` for any value outside `[1, 25]`, including the new-card
  default of `0`.
- `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs` — same fallback
  applied at the wire boundary as a second line of defence.
- The persisted `UserSaveDataGreen.DispTaikojukuDan` column still stores
  the raw user value (`0` for never-configured). Only the wire emission is
  forced to a safe sentinel; if the user later configures a real slot via
  Donder Hiroba, the persisted value updates and naturally falls into the
  `[1, 25]` branch.

## What still needs evidence

- The meaning of `v47` in `sub_7FDFFC:648`. It selects whether the wire
  slot is used or the local `1` default wins. The exact gating condition
  may matter for non-new-card flows (e.g., Donder Hiroba pushes), but the
  new-card crash is unaffected by it because the server always sends
  `1..25` after this fix.
- Whether `disp_taikojuku_dan = 0` was *ever* intended by the original
  protocol (Bandai server) as a meaningful "no choice yet" signal. The
  proto2 source suggests yes (otherwise `required` would have been used),
  but no client code path takes advantage of the distinction.
