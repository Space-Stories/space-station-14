using Content.Shared.Mindshield.Components;
using Content.Shared.Stories.Conversion;
using Content.Shared.Stories.Mindshield;

namespace Content.Server.Stories.Conversion;

public sealed partial class ConversionSystem
{
    // TODO: Имплант не должен защищать от всех конвертаций.
    private void InitializeMindShield()
    {
        base.Initialize();
        SubscribeLocalEvent<MindShieldComponent, ConvertAttemptEvent>(OnConvertAttempt);
        SubscribeLocalEvent<ConversionableComponent, MindShieldImplantedEvent>(OnImplanted);
    }
    private void OnConvertAttempt(EntityUid uid, MindShieldComponent component, ConvertAttemptEvent args)
    {
        args.Cancel();
    }
    private void OnImplanted(EntityUid uid, ConversionableComponent component, MindShieldImplantedEvent args)
    {
        foreach (var (key, conversion) in component.ActiveConversions)
        {
            DoRevert(uid, _prototype.Index(conversion.Prototype));
        }
    }
}
