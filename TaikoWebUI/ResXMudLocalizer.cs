using TaikoWebUI.Localization;
using Microsoft.Extensions.Localization;

namespace TaikoWebUI;


internal class ResXMudLocalizer : MudLocalizer
{
    private IStringLocalizer localization;

    public ResXMudLocalizer(IStringLocalizer<LocalizationResource> localizer)
    {
        localization = localizer;
    }

    public override LocalizedString this[string key] => localization[key];
}