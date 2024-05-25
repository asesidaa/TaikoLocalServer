namespace SharedProject.Models;

public class Costume
{
    public uint CostumeId { get; set; }
    
    public string CostumeType { get; init; } = string.Empty;

    public string CostumeName { get; init; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is Costume costume)
        {
            return costume.CostumeName.Equals(CostumeName) && costume.CostumeType.Equals(CostumeType);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return CostumeName.GetHashCode();
    }
}