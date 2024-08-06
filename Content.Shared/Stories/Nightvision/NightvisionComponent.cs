using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Nightvision;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightvisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = true;
}

[RegisterComponent]
[NetworkedComponent]
public sealed partial class NightvisionClothingComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled")]
    public bool Enabled { get; set; } = true;
}
