using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Skills;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SkillsComponent : Component
{
    [DataField("anySkills"), AutoNetworkedField]
    public bool AnySkills { get; set; } = false;

    [DataField("skills"), AutoNetworkedField]
    public Dictionary<string, FixedPoint2> Skills = new()
    {
        // Combat
        { "Guns", 0.0f },
        { "EnergyGuns", 0.0f },
        { "Melee", 0.0f },
    };

    [DataField("explored"), AutoNetworkedField]
    public HashSet<string> AlreadyExplored = new();
}
