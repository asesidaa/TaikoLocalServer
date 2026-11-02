namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Wire;

[ProtoContract]
public sealed class StartupAuthRequest
{
    [ProtoMember(1, Name = @"chassis_id", IsRequired = true)]
    public string ChassisId { get; set; } = string.Empty;

    [ProtoMember(2, Name = @"usbmem_key")]
    public string? UsbmemKey { get; set; }

    [ProtoMember(3, Name = @"hdd_ver", IsRequired = true)]
    public uint HddVer { get; set; }

    [ProtoMember(4, Name = @"usbmem_ver")]
    public uint? UsbmemVer { get; set; }

    [ProtoMember(5, Name = @"shop_id", IsRequired = true)]
    public string ShopId { get; set; } = string.Empty;

    [ProtoMember(6, Name = @"rack_id")]
    public string? RackId { get; set; }

    [ProtoMember(7, Name = @"country_id")]
    public string? CountryId { get; set; }

    [ProtoMember(8, Name = @"ary_operation_info")]
    public List<OperationData> AryOperationInfoes { get; } = [];

    [ProtoContract]
    public sealed class OperationData
    {
        [ProtoMember(1, Name = @"key_data", IsRequired = true)]
        public uint KeyData { get; set; }

        [ProtoMember(2, Name = @"value_data", IsRequired = true)]
        public byte[] ValueData { get; set; } = [];
    }
}

[ProtoContract]
public sealed class StartupAuthResponse
{
    [ProtoMember(1, Name = @"result", IsRequired = true)]
    public uint Result { get; set; }

    [ProtoMember(2, Name = @"ary_movie_info")]
    public List<MovieData> AryMovieInfoes { get; } = [];

    [ProtoMember(3, Name = @"ary_operation_info")]
    public List<OperationData> AryOperationInfoes { get; } = [];

    [ProtoContract]
    public sealed class MovieData
    {
        [ProtoMember(1, Name = @"movie_id", IsRequired = true)]
        public uint MovieId { get; set; }

        [ProtoMember(2, Name = @"enable_days", IsRequired = true)]
        public uint EnableDays { get; set; }
    }

    [ProtoContract]
    public sealed class OperationData
    {
        [ProtoMember(1, Name = @"key_data", IsRequired = true)]
        public uint KeyData { get; set; }

        [ProtoMember(2, Name = @"value_data", IsRequired = true)]
        public byte[] ValueData { get; set; } = [];
    }
}

[ProtoContract]
public sealed class VerupAuthRequest
{
    [ProtoMember(1, Name = @"chassis_id", IsRequired = true)]
    public string ChassisId { get; set; } = string.Empty;

    [ProtoMember(2, Name = @"usbmem_key", IsRequired = true)]
    public string UsbmemKey { get; set; } = string.Empty;

    [ProtoMember(3, Name = @"hdd_ver", IsRequired = true)]
    public uint HddVer { get; set; }

    [ProtoMember(4, Name = @"usbmem_ver", IsRequired = true)]
    public uint UsbmemVer { get; set; }

    [ProtoMember(5, Name = @"shop_id", IsRequired = true)]
    public string ShopId { get; set; } = string.Empty;

    [ProtoMember(6, Name = @"rack_id")]
    public string? RackId { get; set; }

    [ProtoMember(7, Name = @"country_id")]
    public string? CountryId { get; set; }
}

[ProtoContract]
public sealed class VerupAuthResponse
{
    [ProtoMember(1, Name = @"result", IsRequired = true)]
    public uint Result { get; set; }
}

[ProtoContract]
public sealed class VerupCompleteRequest
{
    [ProtoMember(1, Name = @"chassis_id", IsRequired = true)]
    public string ChassisId { get; set; } = string.Empty;

    [ProtoMember(2, Name = @"usbmem_key", IsRequired = true)]
    public string UsbmemKey { get; set; } = string.Empty;

    [ProtoMember(3, Name = @"hdd_ver", IsRequired = true)]
    public uint HddVer { get; set; }

    [ProtoMember(4, Name = @"usbmem_ver", IsRequired = true)]
    public uint UsbmemVer { get; set; }

    [ProtoMember(5, Name = @"shop_id", IsRequired = true)]
    public string ShopId { get; set; } = string.Empty;

    [ProtoMember(6, Name = @"rack_id")]
    public string? RackId { get; set; }

    [ProtoMember(7, Name = @"country_id")]
    public string? CountryId { get; set; }
}

[ProtoContract]
public sealed class VerupCompleteResponse
{
    [ProtoMember(1, Name = @"result", IsRequired = true)]
    public uint Result { get; set; }
}
