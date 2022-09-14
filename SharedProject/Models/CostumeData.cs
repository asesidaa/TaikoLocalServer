using System.Text.Json.Serialization;

namespace SharedProject.Models;

public class CostumeData
{
    public uint Head { get; set; }

    public uint Body { get; set; }

    public uint Face { get; set; }

    public uint Kigurumi { get; set; }

    public uint Puchi { get; set; }
}