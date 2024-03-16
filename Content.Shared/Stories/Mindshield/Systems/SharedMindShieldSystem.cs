using Content.Shared.Mindshield.Components;

namespace Content.Shared.Stories.Mindshield;

public sealed class SharedMindShieldSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindShieldComponent, MapInitEvent>(MindShieldImplanted);
    }

    private void MindShieldImplanted(EntityUid uid, MindShieldComponent comp, MapInitEvent init)
    {
        var ev = new MindShieldImplantedEvent(uid, comp);
        _entityManager.EventBus.RaiseLocalEvent(uid, ref ev, true);
    }
}

[ByRefEvent]
public readonly record struct MindShieldImplantedEvent(EntityUid Uid, MindShieldComponent MindShield);
