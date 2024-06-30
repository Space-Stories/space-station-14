using Content.Shared.Popups;
using Content.Shared.DoAfter;
using Robust.Shared.Random;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    private const float MaxExp = 1.0f;
    private const float MinExp = 0.0f;
    private const float ExperienceLossProb = 0.5f;
    private readonly HashSet<string> _experienceLossSkills = ["Melee", "Guns", "EnergyGuns"]; // TODO: Remove Hardcode
    public override void Initialize()
    {
        base.Initialize();
        InitializeRequires();
        InitializeReceiving();
        InitializeLoss();
        InitializeAffects();
    }
}
