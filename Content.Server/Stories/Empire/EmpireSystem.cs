using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.SpaceStories.Empire.Components;
using Content.Shared.Stunnable;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.Roles;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server.SpaceStories.Empire;

public sealed class EmpireSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _sharedStun = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmpireMemberRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<HypnotizedEmpireMemberRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<HypnotizedEmpireComponent, MobStateChangedEvent>(OnMobStateChanged);
    }
    private void OnMobStateChanged(EntityUid uid, HypnotizedEmpireComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead && args.NewMobState != MobState.Critical) return;
        Dehypnotize(uid);
    }
    private void OnGetBriefing(EntityUid uid, EmpireMemberRoleComponent comp, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(uid, out var mind) || mind.OwnedEntity == null)
            return;

        args.Append(Loc.GetString("empire-briefing"));
    }
    private void OnGetBriefing(EntityUid uid, HypnotizedEmpireMemberRoleComponent comp, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(uid, out var mind) || mind.OwnedEntity == null)
            return;

        args.Append(Loc.GetString("hypnosis-empire-briefing"));
    }
    public void Hypnotize(EntityUid uid)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind) || HasComp<HypnotizedEmpireComponent>(uid))
            return;
        EnsureComp<HypnotizedEmpireComponent>(uid);
        EnsureComp<EmpireComponent>(uid);
        if (mindId == default || !_role.MindHasRole<HypnotizedEmpireMemberRoleComponent>(mindId))
        {
            _role.MindAddRole(mindId, new HypnotizedEmpireMemberRoleComponent { PrototypeId = "HypnotizedEmpireMember" });
        }
        if (mind?.Session != null)
        {
            var message = Loc.GetString("hypnosis-empire-role-greeting");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
            _chatManager.ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, mind.Session.Channel, Color.Red);
            _audioSystem.PlayGlobal("/Audio/Ambience/Antag/starwars.ogg", uid, new AudioParams() { Volume = -5 });
        }
    }
    public void Dehypnotize(EntityUid uid)
    {
        var stunTime = TimeSpan.FromSeconds(4);
        var name = Identity.Entity(uid, EntityManager);
        _sharedStun.TryParalyze(uid, stunTime, true);
        _popupSystem.PopupEntity(Loc.GetString("rev-break-control", ("name", name)), uid);
        RemComp<EmpireComponent>(uid);
        RemComp<HypnotizedEmpireComponent>(uid);

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;
        if (mindId == default || _role.MindHasRole<HypnotizedEmpireMemberRoleComponent>(mindId))
        {
            _role.MindRemoveRole<HypnotizedEmpireMemberRoleComponent>(mindId);
        }
        if (mind?.Session != null)
        {
            var message = Loc.GetString("hypnosis-empire-role-goodbye");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
            _chatManager.ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, mind.Session.Channel, Color.Red);
        }
    }
}
