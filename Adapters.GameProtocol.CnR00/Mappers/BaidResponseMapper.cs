using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class BaidResponseMapper
{
    public static partial BAIDResponse MapToCN00(CommonBaidResponse commonBaidResponse);

    public static BAIDResponse MapCN00WithPostProcess(CommonBaidResponse commonBaidResponse)
    {
        var response = MapToCN00(commonBaidResponse);
        response.AryCostumedata = new BAIDResponse.CostumeData
        {
            Costume1 = commonBaidResponse.CostumeData[0],
            Costume2 = commonBaidResponse.CostumeData[1],
            Costume3 = commonBaidResponse.CostumeData[2],
            Costume4 = commonBaidResponse.CostumeData[3],
            Costume5 = commonBaidResponse.CostumeData[4]
        };
        response.CostumeFlg1 = commonBaidResponse.CostumeFlagArrays[0];
        response.CostumeFlg2 = commonBaidResponse.CostumeFlagArrays[1];
        response.CostumeFlg3 = commonBaidResponse.CostumeFlagArrays[2];
        response.CostumeFlg4 = commonBaidResponse.CostumeFlagArrays[3];
        response.CostumeFlg5 = commonBaidResponse.CostumeFlagArrays[4];
        return response;
    }
}
