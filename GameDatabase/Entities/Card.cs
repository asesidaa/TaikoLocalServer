namespace GameDatabase.Entities
{
    public partial class Card
    {
        public string AccessCode { get; set; } = null!;
        public ulong Baid { get; set; }
        public string Password { get; set; } = null!;
        public string Salt { get; set; } = null!;
    }
}