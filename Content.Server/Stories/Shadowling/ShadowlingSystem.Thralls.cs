using Content.Server.Popups;
using Content.Server.Radio.Components;
using Content.Server.Stories.Lib;
using Content.Server.Stunnable;
using Content.Shared.IdentityManagement;
using Content.Shared.Mindshield.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Roles;
using Content.Shared.Stories.Mindshield;

namespace Content.Server.Stories.Shadowling;
public sealed partial class ShadowlingSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly StoriesUtilsSystem _utils = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;

    [ValidatePrototypeId<NpcFactionPrototype>]
    public const string ShadowlingNpcFaction = "Shadowling";

    public void InitializeThralls()
    {
        SubscribeLocalEvent<ShadowlingComponent, MindShieldImplantedEvent>(OnMindShieldImplanted);
        SubscribeLocalEvent<ShadowlingThrallComponent, MindShieldImplantedEvent>(OnMindShieldImplanted);
    }

    /// <summary>
    /// Make someone a thrall, set up all needed components (shadowling component, shadowling mind radio)
    /// </summary>
    public void Enthrall(EntityUid target, EntityUid master)
    {
        _npcFaction.AddFaction(target, ShadowlingNpcFaction);
        var slave = EnsureComp<ShadowlingThrallComponent>(target);
        Dirty(target, slave);
        var radio = EnsureComp<ActiveRadioComponent>(target);
        radio.Channels.Add(ShadowlingMindRadioPrototype);
        EnsureComp<ShadowlingThrallRoleComponent>(target);

        var args = new AfterEnthralledEvent(target, master);
        RaiseLocalEvent(target, ref args);
    }

    public void Unthrall(EntityUid target)
    {
        RemCompDeferred<ShadowlingThrallComponent>(target);
        RemCompDeferred<ShadowlingThrallRoleComponent>(target);
        var radio = Comp<ActiveRadioComponent>(target);
        radio.Channels.Remove(ShadowlingMindRadioPrototype);
    }

    private void OnMindShieldImplanted(EntityUid uid, ShadowlingComponent comp, MindShieldImplantedEvent ev)
    {
        RemCompDeferred<MindShieldComponent>(uid);
        _popup.PopupEntity(Loc.GetString("shadowling-break-mindshield"), uid);
    }
    private void OnMindShieldImplanted(EntityUid uid, ShadowlingThrallComponent comp, MindShieldImplantedEvent ev)
    {
        var stunTime = TimeSpan.FromSeconds(4);
        var name = Identity.Entity(uid, EntityManager);

        Unthrall(uid);

        _stun.TryParalyze(uid, stunTime, true);
        _popup.PopupEntity(Loc.GetString("thrall-break-control", ("name", name)), uid);
    }

    public IEnumerable<EntityUid> GetThralls()
    {
        var entities = EntityQueryEnumerator<MobStateComponent>();

        while (entities.MoveNext(out var uid, out var mobState))
        {
            if (mobState.CurrentState == MobState.Alive && IsThrall(uid))
                yield return uid;
        }
    }
}

[ByRefEvent]
public record struct AfterEnthralledEvent(EntityUid Target, EntityUid Master);
