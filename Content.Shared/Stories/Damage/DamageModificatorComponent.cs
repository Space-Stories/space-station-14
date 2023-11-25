using Robust.Shared.GameStates;

namespace Content.Shared.Damage;

/// <summary>
/// Used for clothing that reduces damage when worn.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedDamageModificatorSystem))]
public sealed partial class DamageModificatorComponent : Component
{
    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;
}
