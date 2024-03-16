using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Force.LightSaber;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class LightSaberComponent : Component
{
    /// <summary>
    /// Сущность хозяин меча.
    /// </summary>
    [DataField("lightSaberOwner"), AutoNetworkedField]
    public EntityUid? LightSaberOwner;
}
