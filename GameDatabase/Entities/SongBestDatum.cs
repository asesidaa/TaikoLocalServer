﻿using SharedProject.Enums;

namespace GameDatabase.Entities
{
	public partial class SongBestDatum
	{
		public ulong Baid { get; set; }
		public uint SongId { get; set; }
		public Difficulty Difficulty { get; set; }
		public uint BestScore { get; set; }
		public uint BestRate { get; set; }
		public CrownType BestCrown { get; set; }
		public ScoreRank BestScoreRank { get; set; }

		public virtual Card? Ba { get; set; }
	}
}