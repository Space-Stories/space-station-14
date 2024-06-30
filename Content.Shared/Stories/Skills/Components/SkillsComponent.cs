using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class SkillsComponent : Component
{
    [DataField("anySkills")]
    public bool AnySkills { get; set; } = false;

    // Ограничитель, который например при конфигурации ниже
    // будет уменьшать получаемый опыт в 10 раз после достижения опыта 0.6.
    // TODO: Это должно быть в прототипе скилла для большей кастомизации.

    [DataField("modifiers")]

    public Dictionary<FixedPoint2, FixedPoint2> SkillsModifiers = new()
    {
        { 0.95f, 0.0f }, // Высший уровень владения недостижим в раунде.
    };

    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills = new()
    {
        // Combat
        { "Guns", 0.0f },
        { "EnergyGuns", 0.0f },
        { "Melee", 0.0f },
    };

    [DataField("explored")]
    public HashSet<string> AlreadyExplored = new();
}
