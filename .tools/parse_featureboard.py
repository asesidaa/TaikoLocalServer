from __future__ import annotations

import argparse
import json
import struct
from pathlib import Path


def parse_featureboard(path: Path, offset: int, verup_no: int) -> list[dict[str, object]]:
    data = path.read_bytes()
    if len(data) < offset:
        raise ValueError(f"{path} is shorter than parse offset 0x{offset:x}")

    rows: list[dict[str, object]] = []
    position = offset
    while position < len(data):
        if position + 8 > len(data):
            raise ValueError(f"Truncated row header at offset 0x{position:x}")

        cache_id, song_count = struct.unpack_from(">II", data, position)
        position += 8

        byte_count = song_count * 4
        if position + byte_count > len(data):
            raise ValueError(
                f"Truncated song list for cache id {cache_id} at offset 0x{position:x}"
            )

        songs = list(struct.unpack_from(f">{song_count}I", data, position))
        position += byte_count

        rows.append(
            {
                "folderId": cache_id + 1,
                "verupNo": verup_no,
                "songNo": songs,
            }
        )

    if position != len(data):
        raise ValueError(f"Parser stopped at 0x{position:x}, file length is 0x{len(data):x}")

    return rows


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Convert a Green featureboard.bin cache into runtime event-folder JSON."
    )
    parser.add_argument("--input", default=".tools/featureboard.bin")
    parser.add_argument("--output", default="Host/wwwroot/data/green/green_event_folder_data.json")
    parser.add_argument("--offset", default="0x32")
    parser.add_argument("--verup-no", type=int, default=1)
    args = parser.parse_args()

    input_path = Path(args.input)
    output_path = Path(args.output)
    offset = int(str(args.offset), 0)

    rows = parse_featureboard(input_path, offset, args.verup_no)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(rows, indent=2) + "\n", encoding="utf-8")

    folder_ids = ", ".join(str(row["folderId"]) for row in rows)
    song_counts = ", ".join(str(len(row["songNo"])) for row in rows)
    print(f"Wrote {len(rows)} folders to {output_path}")
    print(f"folderIds: {folder_ids}")
    print(f"songCounts: {song_counts}")


if __name__ == "__main__":
    main()
