namespace GameDatabase.Entities
{
    public partial class Card
    {
        public string AccessCode { get; set; } = null!;
        public ulong Baid { get; set; }
    }
}