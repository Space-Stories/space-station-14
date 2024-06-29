using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Emoting;
using Content.Shared.Gibbing.Events;
using Content.Shared.Hands;
using Content.Shared.Implants;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.Pulling.Events;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stories.Stasis.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared.Stories.Stasis;

/// <summary>
/// Регулирует стан и невозможность действовать от компонента стазиса
/// </summary>
public abstract class SharedStasisSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StandingStateSystem _standingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InStasisComponent, ComponentStartup>(OnStasisAdded);
        SubscribeLocalEvent<InStasisComponent, ComponentShutdown>(OnStasisRemoved);

        // Ловим события
        SubscribeLocalEvent<InStasisComponent, ChangeDirectionAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        SubscribeLocalEvent<InStasisComponent, StandAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, DownAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<InStasisComponent, UseAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, ThrowAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, DropAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, AttackAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, PickupAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, IsEquippingAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, IsUnequippingAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, BeforeDamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<InStasisComponent, StartPullAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, SpeakAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, EmoteAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InStasisComponent, GettingInteractedWithAttemptEvent>(OnInteractionWithAttempt);

        SubscribeLocalEvent<NoStasisComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<NoStasisComponent, GotUnequippedEvent>(OnUnequip);

        SubscribeLocalEvent<StasisStorageComponent, ContainerIsInsertingAttemptEvent>(OnContained);
        SubscribeLocalEvent<StasisStorageComponent, ContainerIsRemovingAttemptEvent>(OnContainRemoved);
    }

    private void OnInteractAttempt(EntityUid uid, InStasisComponent stasised, InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnStasisAdded(EntityUid uid, InStasisComponent stasised, EntityEventArgs args)
    {
        _blocker.UpdateCanMove(uid);
        _audio.PlayPredicted(stasised.StasisSound, uid, null);
        stasised.Effect = Spawn(stasised.EffectEntityProto, Transform(uid).Coordinates);
        _transformSystem.SetParent(stasised.Effect, uid);
    }

    private void OnStasisRemoved(EntityUid uid, InStasisComponent stasised, EntityEventArgs args)
    {
        _blocker.UpdateCanMove(uid);
        _standingSystem.Down(uid);
        _audio.PlayPredicted(stasised.StasisEndSound, uid, null);

        Del(stasised.Effect);
    }

    private void OnAttempt(EntityUid uid, InStasisComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnInteractionWithAttempt(EntityUid uid, InStasisComponent component, GettingInteractedWithAttemptEvent args)
    {
        if (HasComp<StasisImmunityComponent>(args.Uid))
            return;

        args.Cancelled = true;
    }

    private void OnMoveAttempt(EntityUid uid, InStasisComponent component, UpdateCanMoveEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    public bool TryStasis(EntityUid uid, bool refresh, TimeSpan? time = null, StatusEffectsComponent? status = null)
    {
        TimeSpan statusTime;

        if (time.HasValue)
            statusTime = time.Value;
        else
            statusTime = new TimeSpan(1, 0, 0, 0, 0);

        if (!Resolve(uid, ref status, false))
            return false;

        if (HasComp<StasisImmunityComponent>(uid))
            return false;

        if (!_statusEffects.TryAddStatusEffect<InStasisComponent>(uid, "Stasis", statusTime, refresh))
            return false;

        var ev = new StasisEvent();
        RaiseLocalEvent(uid, ref ev);

        _adminLogger.Add(LogType.Stamina, LogImpact.Medium, $"{ToPrettyString(uid):user} был отправлен в стазис");

        return true;
    }

    public void OnDamage(EntityUid uid, InStasisComponent component, ref BeforeDamageChangedEvent args)
    {
        args.Cancelled = true;
    }

    public void OnEquip(EntityUid uid, NoStasisComponent component, GotEquippedEvent args)
    {

        if (TryComp<StaminaComponent>(args.Equipee, out var staminaComp))
        {
            staminaComp.CritThreshold = 1000;
            staminaComp.Decay = 100;
        }

        if (HasComp<StasisImmunityComponent>(args.Equipee))
            return;

        EntityManager.AddComponent<StasisImmunityComponent>(args.Equipee);
    }

    public void OnUnequip(EntityUid uid, NoStasisComponent component, GotUnequippedEvent args)
    {
        if (TryComp<StaminaComponent>(args.Equipee, out var staminaComp))
        {
            staminaComp.CritThreshold = 100;
            staminaComp.Decay = 3;

        }
        if (HasComp<StasisImmunityComponent>(args.Equipee))
            EntityManager.RemoveComponent<StasisImmunityComponent>(args.Equipee);
    }

    public void OnContained(EntityUid uid, StasisStorageComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (HasComp<StasisImmunityComponent>(args.EntityUid))
            return;

        TryStasis(args.EntityUid, true, null, null);
    }

    public void OnContainRemoved(EntityUid uid, StasisStorageComponent component, ContainerIsRemovingAttemptEvent args)
    {
        _statusEffects.TryRemoveStatusEffect(args.EntityUid, "Stasis");
    }
}

/// <summary>
///     Событие когда игрок попадает в стазис
/// </summary>
[ByRefEvent]
public record struct StasisEvent;
