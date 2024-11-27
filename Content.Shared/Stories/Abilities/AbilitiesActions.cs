using System.Numerics;
using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

/// <summary>
/// Abilities - это большой набор способностей, которые не требуют наличия особых компонентов для активации.
/// Конструктор с кучей вложенностей...
/// </summary>

namespace Content.Shared.Stories.Abilities;

public sealed partial class ShootLightningsTargetEvent : EntityTargetActionEvent
{
    [DataField]
    public List<Vector2> Vectors = new()
    {
        new(0, 1),
        new(1, -1),
        new(-1, -1),
    };

    [DataField]
    public EntProtoId LightningPrototype = "Lightning";

    [DataField]
    public bool TriggerLightningEvents = true;
}

public sealed partial class EmpActionEvent : InstantActionEvent
{
    [DataField]
    public float Range = 1.0f;

    [DataField]
    public float EnergyConsumption;

    [DataField]
    public float DisableDuration = 60f;
}

public sealed partial class FreedomActionEvent : InstantActionEvent { }

public sealed partial class RejuvenateActionEvent : InstantActionEvent { }

public sealed partial class FlashAreaEvent : InstantActionEvent
{
    [DataField("duration")]
    public int FlashDuration { get; set; } = 8000;

    [DataField]
    public float Range { get; set; } = 7f;

    [DataField]
    public float SlowTo { get; set; } = 0.5f;

    [DataField]
    public SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/flash.ogg");
}

public sealed partial class PushTargetEvent : EntityTargetActionEvent
{
    [DataField]
    public float ParalyzeTime { get; set; } = 3f;

    [DataField]
    public int Strength { get; set; } = 10;

    [DataField]
    public float DistanceModifier { get; set; } = 2f;

    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class IgniteTargetActionEvent : EntityTargetActionEvent
{
    [DataField]
    public float StackAmount = 1f;
}

public sealed partial class ThrownDashActionEvent : WorldTargetActionEvent
{
    [DataField]
    public float Strength = 10;
}

public sealed partial class RangedGlareEvent : EntityTargetActionEvent
{
    [DataField]
    public int Duration { get; set; } = 15000;

    [DataField]
    public float SlowTo { get; set; } = 0.8f;

    [DataField]
    public SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/flash.ogg");

    [DataField]
    public float RequiredRange { get; set; } = 1.5f;
}
