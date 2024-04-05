using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Content.Shared.Antag;

namespace Content.Shared.SpaceStories.Conversion;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConversionableComponent : Component
{
    [DataField("allowed", required: true), Access(typeof(SharedConversionSystem), Other = AccessPermissions.ReadExecute)]
    public List<string> AllowedConversions = new();
    [DataField("active"), Access(typeof(SharedConversionSystem), Other = AccessPermissions.ReadExecute)]
    [AlwaysPushInheritance]
    public Dictionary<string, ConversionPrototype> ActiveConversions = new();
}
