namespace TaikoLocalServer.Domain.Entities;

public partial class UserDatum
{
    public uint Baid { get; set; }
    public string MyDonName { get; set; } = string.Empty;
    public uint MyDonNameLanguage { get; set; }
    public bool IsAdmin { get; set; }
    public List<Token> Tokens { get; set; } = new();
}
