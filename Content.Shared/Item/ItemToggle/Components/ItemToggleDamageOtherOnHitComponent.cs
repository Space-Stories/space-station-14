// SpaceStories

using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Content.Shared.Damage;

namespace Content.Shared.Item;

/// <summary>
/// Handles the changes to the melee weapon component when the item is toggled.
/// </summary>
/// <remarks>
/// You can change the damage, sound on hit, on swing, as well as hidden status while activated.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ItemToggleDamageOtherOnHitComponent : Component
{
    /// <summary>
    ///     The noise this item makes when hitting something with it on.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? ActivatedSoundOnHit;

    /// <summary>
    ///     The noise this item makes when hitting something with it off.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? DeactivatedSoundOnHit;

    /// <summary>
    ///     Damage done by this item when activated.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public DamageSpecifier? ActivatedDamage = null;

    /// <summary>
    ///     Damage done by this item when deactivated.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public DamageSpecifier? DeactivatedDamage = null;
}
