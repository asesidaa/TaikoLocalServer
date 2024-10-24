namespace SharedProject.Models;

public class UserAppearance
{
    public uint Baid { get; set; }
    public string MyDonName { get; set; } = string.Empty;
    public uint MyDonNameLanguage { get; set; }
    public string Title { get; set; } = string.Empty;
    public uint TitlePlateId { get; set; }
    public uint Kigurumi { get; set; }
    public uint Head { get; set; }
    public uint Body { get; set; }
    public uint Face { get; set; }
    public uint Puchi { get; set; }
    public uint FaceColor { get; set; }
    public uint BodyColor { get; set; }
    public uint LimbColor { get; set; }
}
