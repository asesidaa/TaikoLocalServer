using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class EventFolderData
{
	[JsonPropertyName("folderId")]
	public uint FolderId { get; set; }

	[JsonPropertyName("verupNo")]
	public uint VerupNo { get; set; }

	[JsonPropertyName("priority")]
	public uint Priority { get; set; }

	[JsonPropertyName("songNo")]
	public uint[]? SongNo { get; set; }

	[JsonPropertyName("parentFolderId")]
	public uint ParentFolderId { get; set; }
}