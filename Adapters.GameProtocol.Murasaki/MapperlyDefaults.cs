using Riok.Mapperly.Abstractions;

[assembly: MapperDefaults(
    AutoUserMappings = false,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
