namespace SharedProject.Models;

public class Costume
{
    public uint CostumeId { get; set; }
    
    public string CostumeType { get; init; } = string.Empty;

    public string CostumeName { get; init; } = string.Empty;
    public string CostumeNameEN { get; init; } = string.Empty;
    public string CostumeNameCN { get; init; } = string.Empty;
    public string CostumeNameKO { get; init; } = string.Empty;
}