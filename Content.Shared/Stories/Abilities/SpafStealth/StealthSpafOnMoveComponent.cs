using Robust.Shared.GameStates;

namespace Content.Shared.Abilities.SpafStealth;

[RegisterComponent, NetworkedComponent]
public sealed partial class StealthSpafOnMoveComponent : Component
{

    /// <summary>
    /// Rate that effects how fast an entity's visibility passively changes.
    /// </summary>
    [DataField("passiveSVisibilityRate")]
    public float PassiveSVisibilityRate = -0.15f;

    /// <summary>
    /// Rate for movement induced visibility changes. Scales with distance moved.
    /// </summary>
    [DataField("movementSVisibilityRate")]
    public float MovementSVisibilityRate = 0.2f;
}
