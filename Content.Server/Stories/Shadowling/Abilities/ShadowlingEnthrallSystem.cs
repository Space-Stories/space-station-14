using Content.Server.DoAfter;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Stories.Lib;
using Content.Shared.Body.Components;
using Content.Shared.CCVar;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Stories.Shadowling;
using Robust.Shared.Configuration;

namespace Content.Server.Stories.Shadowling;
public sealed class ShadowlingEnthrallSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly StoriesUtilsSystem _utils = default!;

    private readonly List<string> _enthrallablePrototypes = new()
    {
        "Arachnid",
        "Diona",
        "Dwarf",
        "Human",
        "Moth",
        "Reptilian",
        "Slime",
        "Kidan"
    };
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingEnthrallEvent>(OnEnthrallEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHypnosisEvent>(OnHypnosisEvent);
        SubscribeLocalEvent<ShadowlingComponent, EnthrallDoAfterEvent>(OnEnthrallDoAfterEvent);
    }

    private void OnEnthrallEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingEnthrallEvent ev)
    {
        if (ev.Handled)
            return;

        if (_shadowling.IsShadowling(ev.Target) || _shadowling.IsThrall(ev.Target))
        {
            _popup.PopupEntity("Вы не можете порабощать своих союзников", uid, uid);
            return;
        }

        // You cannot enthrall someone with wrong body
        if (!TryComp<BodyComponent>(ev.Target, out var body) || body.Prototype == null || !_enthrallablePrototypes.Contains(body.Prototype.Value.Id))
            return;
        // You cannot enthrall someone without mind
        var shadowlingEnthrallRequireMindAvailability = _config.GetCVar(CCVars.ShadowlingEnthrallRequireMindAvailability);
        if (
            shadowlingEnthrallRequireMindAvailability && !_mind.TryGetMind(ev.Target, out var mindId, out var mind)
        )
        {
            _popup.PopupEntity("Вы можете порабощать существ только в сознании", uid, uid);
            return;
        }

        ev.Handled = true;

        if (TryComp<MindShieldComponent>(ev.Target, out _))
        {
            _popup.PopupEntity("Вы поглощаете чей-то разум... Некий барьер полностью отражает вашу атаку", ev.Performer, ev.Performer);
            _popup.PopupEntity("Ваш разум поглощается тенями... Но некий барьер полностью изгоняет их из вашего разума", ev.Target, ev.Target);
            _stamina.TryTakeStamina(ev.Performer, 50);
            return;
        }

        _popup.PopupEntity("Вы поглощаете чей-то разум...", ev.Performer, ev.Performer);
        _popup.PopupEntity("Ваш разум поглощается тенями...", ev.Target, ev.Target);

        var doAfter = new DoAfterArgs(EntityManager, ev.Performer, 5, new EnthrallDoAfterEvent(), ev.Performer, ev.Target)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnHypnosisEvent(EntityUid uid, ShadowlingComponent component, ref ShadowlingHypnosisEvent ev)
    {
        ev.Handled = false;
        if (TryComp<ShadowlingComponent>(ev.Target, out _))
            return;

        // You cannot enthrall someone without body
        if (!TryComp<BodyComponent>(ev.Target, out _))
            return;
        // You cannot enthrall someone without mind
        var shadowlingEnthrallRequireMindAvailability = _config.GetCVar(CCVars.ShadowlingEnthrallRequireMindAvailability);
        if (
            shadowlingEnthrallRequireMindAvailability &&
            !_utils.IsInConsciousness(ev.Target)
        )
        {
            _popup.PopupEntity("Вы можете порабощать существ только в сознании", uid, uid);
            return;
        }

        ev.Handled = true;

        if (TryComp<MindShieldComponent>(ev.Target, out _))
        {
            _popup.PopupEntity("Некий барьер полностью отражает вашу атаку", ev.Performer, ev.Performer);
            _popup.PopupEntity("Некий барьер отразил сильнейшую ментальную атаку", ev.Target, ev.Target);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, ev.Performer, 0, new EnthrallDoAfterEvent(), ev.Target)
        {
            BlockDuplicate = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnEnthrallDoAfterEvent(EntityUid uid, ShadowlingComponent shadowling, ref EnthrallDoAfterEvent ev)
    {
        if (ev.Target is not { } target)
            return;

        if (ev.Cancelled)
            return;

        _popup.PopupEntity("Ваш разум поглощён тенями", target, target);
        _popup.PopupEntity("Вы стали чуть сильнее", ev.User, ev.User);
        _stamina.TakeStaminaDamage(target, 100);

        _shadowling.Enthrall(target, uid);
    }
}
