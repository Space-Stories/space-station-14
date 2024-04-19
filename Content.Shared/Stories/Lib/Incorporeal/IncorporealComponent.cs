using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Lib.Incorporeal;

/// <summary>
/// Component which indicates is entity incorporeal
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IncorporealComponent : Component
{
    [DataField("speedModifier"), ViewVariables(VVAccess.ReadWrite)]
    public float SpeedModifier = 3f;

    [DataField("collisionMaskBefore"), ViewVariables(VVAccess.ReadOnly)]
    public int CollisionMaskBefore;

    [DataField("collisionLayerBefore"), ViewVariables(VVAccess.ReadOnly)]
    public int CollisionLayerBefore;
}
