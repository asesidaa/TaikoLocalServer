namespace GameDatabase.Entities
{
    public partial class Credential
    {
        public uint Baid { get; set; }
        public string Password { get; set; } = null!;
        public string Salt { get; set; } = null!;

        public virtual UserDatum? Ba { get; set; }
    }
}