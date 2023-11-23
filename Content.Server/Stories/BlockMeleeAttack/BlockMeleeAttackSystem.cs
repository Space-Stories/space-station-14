using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Audio;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Administration.Logs;
using Content.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Containers;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.SpaceStories.BlockMeleeAttack;

public sealed partial class BlockMeleeAttackSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BlockMeleeAttackUserComponent, DamageModifyEvent>(OnUserDamageModified);
        SubscribeLocalEvent<BlockMeleeAttackComponent, GotEquippedHandEvent>(OnReflectHandEquipped);
        SubscribeLocalEvent<BlockMeleeAttackComponent, GotUnequippedHandEvent>(OnReflectHandUnequipped);
        // SubscribeLocalEvent<BlockingComponent, DamageModifyEvent>(OnDamageModified);
    }

    private void OnUserDamageModified(EntityUid uid, BlockMeleeAttackUserComponent component, DamageModifyEvent args)
    {
        if (TryComp<BlockMeleeAttackComponent>(component.BlockingItem, out var blocking))
        {
            if (args.Damage.GetTotal() <= 0) return;
            if (args.Origin == uid || args.Origin == component.BlockingItem) return;

            if (!_random.Prob(blocking.BlockProb)) return;

            args.Damage *= 0;
            _popup.PopupEntity(Loc.GetString("Заблокировано!"), component.BlockingItem.Value);
            _audio.PlayPvs(blocking.BlockSound, uid);
        }
    }

    private void OnReflectHandEquipped(EntityUid uid, BlockMeleeAttackComponent component, GotEquippedHandEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        component.User = args.User;
        EnsureComp<BlockMeleeAttackUserComponent>(args.User).BlockingItem = uid;
    }

    private void OnReflectHandUnequipped(EntityUid uid, BlockMeleeAttackComponent component, GotUnequippedHandEvent args)
    {
        if (component.User != null) RemComp<BlockMeleeAttackUserComponent>(component.User.Value);
    }

    // private void OnDamageModified(EntityUid uid, BlockingComponent component, DamageModifyEvent args)
    // {
    //     var modifier = component.IsBlocking ? component.ActiveBlockDamageModifier : component.PassiveBlockDamageModifer;
    //     if (modifier == null)
    //     {
    //         return;
    //     }

    //     args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifier);
    // }
}
