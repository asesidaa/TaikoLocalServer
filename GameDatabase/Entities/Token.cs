namespace GameDatabase.Entities;

public class Token
{
    public uint Baid { get; set; }
    
    public int Id { get; set; }

    public int Count { get; set; }

    public virtual UserDatum? Datum { get; set; }
}