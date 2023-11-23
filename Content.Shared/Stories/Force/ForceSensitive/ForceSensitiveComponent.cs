namespace Content.Shared.SpaceStories.Force.ForceSensitive;
using Content.Shared.Actions;

[RegisterComponent]
public sealed partial class ForceSensitiveComponent : Component
{
    [Dependency] private IEntityManager _entityManager;

    /// <summary>
    /// Световой меч чувствительного к силе.
    /// </summary>
    [DataField("lightsaber"), AutoNetworkedField]
    public EntityUid? LightSaber;

    /// <summary>
    /// Какая связь у существа с силой.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("forceType")]
    public ForceType ForceType
    {
        get
        {
            return _forceType;
        }
        set
        {
            _forceType = value;
            Actions.TryGetValue(value, out var toGrant);
            var ev = new ForceTypeChangeEvent(Owner, value, toGrant);
            _entityManager.EventBus.RaiseLocalEvent(Owner, ref ev, true);
        }
    }
    private ForceType _forceType = ForceType.Jedi;

    /// <summary>
    /// Способности у существа в зависимости от типа силы.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("actions")]
    public Dictionary<ForceType, List<string>> Actions = new()
    {
// {ForceType.Jedi, new() {"ActionRecallLightSaber", "ActionProtectiveBubble", "ActionTelekinesis", "ActionStaminaPush"}},
{ForceType.Jedi, new() {"ActionRecallLightSaber", "ActionTelekinesis", "ActionPushBall", "ActionCreateProtectiveBubble"}} // "ActionTelekinesis", "ActionStaminaPush"
    };

    /// <summary>
    /// Способности которые сила дала существу.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("grantedActions")]
    public List<EntityUid> GrantedActions = new();
}
public enum ForceType : byte
{
    HighSith, // All sith's actions
    Sith, // Some sith's actions
    GreyJedi, // Some jedi's actions and a little sith's actions
    Jedi, // Some jedi's actions
    HighJedi, // All jedi's actions
    Overlord // Absolute power!
}

[ByRefEvent]
public readonly record struct ForceTypeChangeEvent(EntityUid Uid, ForceType ForceType, List<string>? NewActions);
public sealed partial class RecallLightSaberEvent : InstantActionEvent
{
}

public sealed partial class HandTetherGunEvent : InstantActionEvent
{
}
public sealed partial class CreateProtectiveBubbleEvent : InstantActionEvent
{
}

public sealed partial class StopProtectiveBubbleEvent : InstantActionEvent
{
}
