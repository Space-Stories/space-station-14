using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Reflectors;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReflectCountComponent : Component
{

    [DataField]
    public int ReflectionsCount { get; set; }

    [DataField]
    public int MaxReflections { get; set; } = 12;
}
