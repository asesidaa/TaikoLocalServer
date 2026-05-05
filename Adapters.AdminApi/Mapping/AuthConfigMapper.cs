using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Mapping;

[Mapper]
public static partial class AuthConfigMapper
{
    public static partial ClientAuthConfigResponse ToResponse(this AuthSettings settings);
}
