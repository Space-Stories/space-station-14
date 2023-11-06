using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed class ShowHealthIconsSystem : EquipmentHudSystem<ShowHealthIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, MobStateComponent mobStateComponent, ref GetStatusIconsEvent args)
    {
        if (!IsActive || args.InContainer)
            return;

        var healthIcons = DecideHealthIcon(mobStateComponent);

        args.StatusIcons.AddRange(healthIcons);
    }

    private IReadOnlyList<StatusIconPrototype> DecideHealthIcon(MobStateComponent mobStateComponent)
    {
        var result = new List<StatusIconPrototype>();

        switch (mobStateComponent.CurrentState)
        {
            case MobState.Alive:
                if (_prototypeMan.TryIndex<StatusIconPrototype>("HealthIconAlive", out var alive))
                {
                    result.Add(alive);
                }
                break;
            case MobState.Critical:
                if (_prototypeMan.TryIndex<StatusIconPrototype>("HealthIconCritical", out var critical))
                {
                    result.Add(critical);
                }
                break;
            case MobState.Dead:
                if (_prototypeMan.TryIndex<StatusIconPrototype>("HealthIconDead", out var dead))
                {
                    result.Add(dead);
                }
                break;
        }

        return result;
    }
}
