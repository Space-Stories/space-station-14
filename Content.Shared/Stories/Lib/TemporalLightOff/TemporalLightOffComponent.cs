using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Lib.TemporalLightOff;

[RegisterComponent, NetworkedComponent]
public sealed partial class TemporalLightOffComponent : Component
{
    [DataField, ViewVariables]
    public TimeSpan LightOffFor = TimeSpan.FromMinutes(2);

    [DataField, ViewVariables]
    public TimeSpan EnableLightAt = TimeSpan.Zero;
}
