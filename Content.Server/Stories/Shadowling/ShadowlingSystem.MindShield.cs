using Content.Shared.Mindshield.Components;
using Content.Shared.Stories.Mindshield;
using Content.Shared.Stories.Shadowling;

namespace Content.Server.Stories.Shadowling;
public sealed partial class ShadowlingSystem
{
    public void InitializeMindShield()
    {
        SubscribeLocalEvent<ShadowlingComponent, MindShieldImplantedEvent>(OnMindShieldImplanted);
    }
    private void OnMindShieldImplanted(EntityUid uid, ShadowlingComponent component, MindShieldImplantedEvent args)
    {
        RemCompDeferred<MindShieldComponent>(uid);
        _popup.PopupEntity(Loc.GetString("shadowling-break-mindshield"), uid);
    }
}
