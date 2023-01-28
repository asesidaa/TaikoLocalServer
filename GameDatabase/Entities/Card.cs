namespace GameDatabase.Entities;

public class Card
{
    public string AccessCode { get; set; } = null!;
    public uint Baid { get; set; }
    public string Password { get; set; } = null!;
    public string Salt { get; set; } = null!;
}