using Content.Shared.Atmos.Miasma;
using Content.Shared.Damage;
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
    [Dependency] private readonly IEntityManager _entity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, MobStateComponent mobStateComponent, ref GetStatusIconsEvent args)
    {
        if (!IsActive || args.InContainer)
            return;

        var damageable = _entity.GetComponent<DamageableComponent>(uid);
        if (damageable.DamageContainerID != "Biological")
            return;

        var healthIcons = DecideHealthIcon(uid, mobStateComponent);

        args.StatusIcons.AddRange(healthIcons);
    }

    private IReadOnlyList<StatusIconPrototype> DecideHealthIcon(EntityUid uid, MobStateComponent mobStateComponent)
    {
        var result = new List<StatusIconPrototype>();

        switch (mobStateComponent.CurrentState)
        {
            case MobState.Alive:
            case MobState.Critical:
                if (_prototypeMan.TryIndex<StatusIconPrototype>("HealthStateIconNormal", out var alive))
                {
                    result.Add(alive);
                }
                break;
            case MobState.Dead:
                var isRotting = _entity.GetComponentOrNull<RottingComponent>(uid) != null;

                if (!isRotting && _prototypeMan.TryIndex<StatusIconPrototype>("HealthStateIconDefib", out var defib))
                {
                    result.Add(defib);
                }
                else if (_prototypeMan.TryIndex<StatusIconPrototype>("HealthStateIconDead", out var dead))
                {
                    result.Add(dead);
                }
                break;
        }

        return result;
    }
}
