using Content.Shared.Popups;
using Content.Shared.DoAfter;
using Robust.Shared.Random;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    private const float MaxExp = 1.0f;
    private const float MinExp = 0.0f;
    public override void Initialize()
    {
        base.Initialize();
        InitializeRequires();
        InitializeReceiving();
        InitializeAffects();
    }
}
