﻿using SharedProject.Enums;

namespace GameDatabase.Entities;

public partial class SongBestDatum
{
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public uint BestScore { get; set; }
    public uint BestRate { get; set; }
    public CrownType BestCrown { get; set; }
    public ScoreRank BestScoreRank { get; set; }
    
    public uint BestGoodCount { get; set; }
    
    public uint BestOkCount { get; set; }
    
    public uint BestMissCount { get; set; }
    
    public uint BestComboCount { get; set; }
    
    public uint BestHitCount { get; set; }
    
    public uint BestDrumrollCount { get; set; }
    public short Option { get; set; }

    public virtual Card? Ba { get; set; }
}