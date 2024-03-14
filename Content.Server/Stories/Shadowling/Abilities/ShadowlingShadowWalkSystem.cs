using Content.Server.Stories.Lib.Incorporeal;
using Content.Shared.Stories.Shadowling;
using Robust.Shared.Timing;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingShadowWalkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IncorporealSystem _incorporeal = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingShadowWalkEvent>(OnShadowWalkEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingPlaneShiftEvent>(OnPlaneShiftEvent);
    }

    private void OnShadowWalkEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingShadowWalkEvent ev)
    {
        if (ev.Handled)
            return;
        ev.Handled = true;

        if (!component.InShadowWalk)
            BeginShadowWalk(uid, component);
        else
            EndShadowWalk(uid, component);
    }

    private void OnPlaneShiftEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingPlaneShiftEvent ev)
    {
        if (ev.Handled)
            return;
        ev.Handled = true;

        if (!component.InShadowWalk)
            BeginPlaneShift(uid);
        else
            EndPlaneShift(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ShadowlingComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.InShadowWalk && curTime > comp.ShadowWalkEndsAt)
            {
                EndShadowWalk(uid, comp);
            }
        }
    }

    private void BeginShadowWalk(EntityUid uid, ShadowlingComponent shadowling)
    {
        var curTime = _timing.CurTime;
        shadowling.ShadowWalkEndsAt = curTime.Add(shadowling.ShadowWalkEndsIn);
        shadowling.InShadowWalk = true;
        Dirty(uid, shadowling);

        _incorporeal.MakeIncorporeal(uid);
    }

    private void EndShadowWalk(EntityUid uid, ShadowlingComponent shadowling)
    {
        shadowling.InShadowWalk = false;
        Dirty(uid, shadowling);

        _incorporeal.MakeCorporeal(uid);
    }

    private void BeginPlaneShift(EntityUid uid)
    {
        _incorporeal.MakeIncorporeal(uid);
    }

    private void EndPlaneShift(EntityUid uid)
    {
        _incorporeal.MakeCorporeal(uid);
    }
}
