namespace Content.Shared.Stories.Shadowling;
public abstract class SharedShadowlingSystem<TThrallComponent, TShadowlingComponent> : EntitySystem
    where TThrallComponent : SharedShadowlingThrallComponent
    where TShadowlingComponent : SharedShadowlingComponent
{
    public bool IsThrall(EntityUid uid)
    {
        return HasComp<TThrallComponent>(uid);
    }

    public bool IsShadowling(EntityUid uid)
    {
        return HasComp<TShadowlingComponent>(uid);
    }
}
