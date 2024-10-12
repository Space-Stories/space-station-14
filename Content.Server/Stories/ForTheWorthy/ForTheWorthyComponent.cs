using Content.Shared.Damage;

namespace Content.Server.Stories.ForTheWorthy;

[RegisterComponent]
public sealed partial class ForTheWorthyComponent : Component
{
    [DataField("selfDamage", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier SelfDamage = default!;
}
