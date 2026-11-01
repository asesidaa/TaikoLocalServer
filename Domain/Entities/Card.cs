namespace TaikoLocalServer.Domain.Entities
{
    public partial class Card
    {
        public string AccessCode { get; set; } = null!;
        public uint Baid { get; set; }
        /// <summary>The player's name for it (Settings › Your cards), e.g. "Banapass"; empty = none.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>When a cabinet last read it (UTC).</summary>
        public DateTime? LastScanAt { get; set; }

        public virtual UserDatum? Ba { get; set; }
    }
}