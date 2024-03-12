using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Server.Atmos.Components;
using Content.Server.Stories.Force.Components;
using Content.Shared.Weapons.Reflect;
using Content.Shared.Stories.Force.ForceSensitive;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Explosion;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.Actions;

namespace Content.Server.Stories.Force.Systems;

public sealed class ForceProtectiveBubbleSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ForceProtectiveBubbleComponent, DamageModifyEvent>(OnDamage);
        SubscribeLocalEvent<ForceProtectiveBubbleComponent, ComponentStartup>(OnMarkerStartup);
        SubscribeLocalEvent<ForceProtectiveBubbleComponent, ComponentShutdown>(OnMarkerShutdown);
        SubscribeLocalEvent<ForceProtectiveBubbleComponent, GetExplosionResistanceEvent>(OnGetExplosionResistance);
        SubscribeLocalEvent<ForceProtectiveBubbleComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<ForceSensitiveComponent, CreateProtectiveBubbleEvent>(OnProtectiveBubble);
        SubscribeLocalEvent<ForceProtectiveBubbleComponent, StopProtectiveBubbleEvent>(OnStopProtectiveBubble);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ForceProtectiveBubbleComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            DamageBubble(uid, frameTime * 0.5f, component);
        }
    }
    private void OnMobStateChanged(EntityUid uid, ForceProtectiveBubbleComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive || args.NewMobState == MobState.Invalid) return;
        RemComp<ForceProtectiveBubbleComponent>(uid);
        OnMarkerShutdown(uid, comp);
    }
    private void OnGetExplosionResistance(EntityUid uid, ForceProtectiveBubbleComponent component, ref GetExplosionResistanceEvent args)
    {
        args.DamageCoefficient = component.ExplosionResistance;
    }
    private void OnMarkerStartup(EntityUid uid, ForceProtectiveBubbleComponent comp, ComponentStartup args)
    {
        _actions.AddAction(uid, ref comp.StopProtectiveBubbleActionEntity, out var act, comp.StopProtectiveBubbleAction);

        if (TryComp<BarotraumaComponent>(uid, out var barotrauma)) barotrauma.HasImmunity = true;

        comp.EffectEntity = Spawn(comp.EffectEntityProto, Transform(uid).Coordinates);
        _transformSystem.SetParent(comp.EffectEntity, uid);

        var reflect = EnsureComp<ReflectComponent>(uid);
        reflect.Enabled = true;
        reflect.ReflectProb = 1f;
        reflect.Reflects = ReflectType.Energy;

        if (comp.SoundLoop != null)
            comp.PlayingStream = _audio.PlayPvs(comp.SoundLoop, uid, comp.SoundLoop.Params).Value.Entity;
    }
    private void OnMarkerShutdown(EntityUid uid, ForceProtectiveBubbleComponent comp, ComponentShutdown? args = null)
    {
        if (comp.StopProtectiveBubbleActionEntity != null) _actions.RemoveAction(uid, comp.StopProtectiveBubbleActionEntity);

        comp.PlayingStream = _audio.Stop(comp.PlayingStream);

        var reflect = EnsureComp<ReflectComponent>(uid);
        reflect.Enabled = false;

        Del(comp.EffectEntity);

        if (TryComp<BarotraumaComponent>(uid, out var barotrauma)) barotrauma.HasImmunity = false;
        if (comp.StopSound != null) _audio.PlayPvs(comp.StopSound, uid, comp.StopSound.Params);
    }
    private void OnDamage(EntityUid uid, ForceProtectiveBubbleComponent component, DamageModifyEvent args)
    {
        if (args.Damage.GetTotal() <= 0) return;
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, component.Modifiers);
        DamageBubble(uid, args.Damage.GetTotal().Value / 100);
    }
    private void OnStopProtectiveBubble(EntityUid uid, ForceProtectiveBubbleComponent comp, StopProtectiveBubbleEvent args)
    {
        RemComp<ForceProtectiveBubbleComponent>(uid);
    }

    private void OnProtectiveBubble(EntityUid uid, ForceSensitiveComponent comp, CreateProtectiveBubbleEvent args)
    {
        if (args.Handled) return;
        args.Handled = true;
        var user = args.Performer;
        if (!TryComp<ForceSensitiveComponent>(user, out var ninja))
            return;

        EnsureComp<ForceProtectiveBubbleComponent>(user);

        _popup.PopupClient(Loc.GetString("Ваше тело покрывает защитный пузырь..."), user, user);
    }
    private void DamageBubble(EntityUid uid, float damage, ForceProtectiveBubbleComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.Health -= damage;

        if (comp.Health <= 0) RemComp<ForceProtectiveBubbleComponent>(uid);
    }
}
