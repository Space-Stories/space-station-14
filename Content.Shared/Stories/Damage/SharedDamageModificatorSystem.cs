using Content.Shared.Inventory;

namespace Content.Shared.Damage;

/// <summary>
/// This handles logic relating to <see cref="DamageModificatorComponent"/>
/// </summary>
public abstract class SharedDamageModificatorSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageModificatorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
    }

    private void OnDamageModify(EntityUid uid, DamageModificatorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        Log.Debug("Modified damage");
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, component.Modifiers);
    }
}
