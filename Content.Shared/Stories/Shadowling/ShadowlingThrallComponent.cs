using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingThrallComponent : Component
{
    [DataField]
    public Color OldEyeColor = Color.Black;
}
