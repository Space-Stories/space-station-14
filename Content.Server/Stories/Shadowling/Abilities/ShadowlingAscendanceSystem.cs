using Content.Server.Chat.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Polymorph.Systems;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Stunnable;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.Standing;
using Content.Shared.Stories.Shadowling;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Player;

namespace Content.Server.Stories.Shadowling;

public sealed class ShadowlingAscendanceSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public readonly string ShadowlingAscendedPolymorph = "Ascended";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceEvent>(OnAscendance);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceDoAfterEvent>(OnAscendanceDoAfter);
    }

    private void OnAscendance(EntityUid uid, ShadowlingComponent component, ref ShadowlingAscendanceEvent ev)
    {
        if (ev.Handled)
            return;

        ev.Handled = true;
        Log.Debug("Point 0");

        var solution = new Solution();
        solution.AddReagent("ShadowlingSmokeReagent", 100);

        // var smokeEnt = Spawn("Smoke", Transform(uid).Coordinates);
        // _smoke.StartSmoke(smokeEnt, solution, 5, 7);

        var newNullableUid = _polymorph.PolymorphEntity(uid, ShadowlingAscendedPolymorph);

        if (newNullableUid is not { } newUid)
            return;

        _stun.TryStun(newUid, TimeSpan.FromSeconds(5), true);
        _standing.Down(newUid, dropHeldItems: false);
        _physics.SetBodyType(newUid, BodyType.Static);

        var doAfter = new DoAfterArgs(EntityManager, newUid, 5, new ShadowlingAscendanceDoAfterEvent(), newUid)
        {
            RequireCanInteract = false,
        };
        _doAfter.TryStartDoAfter(doAfter);

        var announcementString = "Сканерами дальнего действия было зафиксировано превознесение тенеморфа, к вам будет отправлен экстренный эвакуационный шаттл.";

        _chat.DispatchGlobalAnnouncement(announcementString, playSound: true, colorOverride: Color.Red);
        _audio.PlayGlobal("/Audio/Stories/Misc/tear_of_veil.ogg", Filter.Broadcast(), true, AudioParams.Default.WithVolume(-2f));
        _roundEnd.RequestRoundEnd(TimeSpan.FromMinutes(3), newUid, false);
    }

    private void OnAscendanceDoAfter(EntityUid uid, ShadowlingComponent component, ref ShadowlingAscendanceDoAfterEvent ev)
    {
        _standing.Stand(uid);
        _physics.SetBodyType(uid, BodyType.KinematicController);
    }
}
