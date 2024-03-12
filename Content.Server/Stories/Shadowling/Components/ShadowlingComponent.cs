using Content.Shared.Damage;
using Content.Shared.Stories.Shadowling;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Stories.Shadowling;

[RegisterComponent]
public sealed partial class ShadowlingComponent : SharedShadowlingComponent
{
    [ViewVariables(VVAccess.ReadOnly), DataField("inShadowWalk")]
    public bool InShadowWalk;

    [ViewVariables(VVAccess.ReadOnly), DataField("shadowWalkEndsAt", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan ShadowWalkEndsAt = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly), DataField("shadowWalkEndsIn", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan ShadowWalkEndsIn = TimeSpan.FromSeconds(3);

    [ViewVariables(VVAccess.ReadOnly), DataField("grantedActions")]
    public Dictionary<string, EntityUid> GrantedActions = new();

    [DataField("darknessHealing")]
    public DamageSpecifier DarknessHealing = new()
    {
        DamageDict = new()
        {
            { "Blunt", -5 },
            { "Slash", -5 },
            { "Piercing", -5 },
            { "Heat", -5 },
            { "Shock", -5 }
        }
    };

    [DataField("lightnessDamage")]
    public DamageSpecifier LightnessDamage = new()
    {
        DamageDict = new()
        {
            { "Heat", 1 }
        }
    };

    [ViewVariables(VVAccess.ReadOnly), DataField("ascended")]
    public bool Ascended = false;

    [ViewVariables(VVAccess.ReadWrite), DataField("performLightDamage")]
    public bool PerformLightDamage = false;

    [ViewVariables(VVAccess.ReadWrite), DataField("debugDisableThrallsCountCheck")]
    public bool DebugDisableThrallsCountCheck = false;
}
