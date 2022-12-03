using System.Collections.Specialized;
using SharedProject.Enums;
using SharedProject.Models;
using Throw;

namespace SharedProject.Utils;

public static class PlaySettingConverter
{
    public static PlaySetting ShortToPlaySetting(short input)
    {
        var bits = new BitVector32(input);
        var speedSection = BitVector32.CreateSection(15);
        var vanishSection = BitVector32.CreateSection(1, speedSection);
        var inverseSection = BitVector32.CreateSection(1, vanishSection);
        var randomSection = BitVector32.CreateSection(2, inverseSection);

        var randomType = (RandomType)bits[randomSection];
        randomType.Throw().IfOutOfRange();
        var result = new PlaySetting
        {
            Speed = (uint)bits[speedSection],
            IsVanishOn = bits[vanishSection] == 1,
            IsInverseOn = bits[inverseSection] == 1,
            RandomType = randomType
        };

        return result;
    }

    public static short PlaySettingToShort(PlaySetting setting)
    {
        var bits = new BitVector32();
        var speedSection = BitVector32.CreateSection(15);
        var vanishSection = BitVector32.CreateSection(1, speedSection);
        var inverseSection = BitVector32.CreateSection(1, vanishSection);
        var randomSection = BitVector32.CreateSection(2, inverseSection);

        bits[speedSection] = (int)setting.Speed;
        bits[vanishSection] = setting.IsVanishOn ? 1 : 0;
        bits[inverseSection] = setting.IsInverseOn ? 1 : 0;
        bits[randomSection] = (int)setting.RandomType;

        return (short)bits.Data;
    }
}