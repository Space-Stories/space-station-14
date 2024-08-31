using Content.Shared.Access.Components;
using Content.Shared.Stories.ChameleonStamp;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Content.Shared.Paper;

namespace Content.Shared.Stories.ChameleonStamp;

public abstract class SharedChameleonStampSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedItemSystem _itemSys = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChameleonStampComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<ChameleonStampComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid uid, ChameleonStampComponent component, GotEquippedEvent args)
    {
        component.User = args.Equipee;
    }

    private void OnGotUnequipped(EntityUid uid, ChameleonStampComponent component, GotUnequippedEvent args)
    {
        component.User = null;
    }

    public void CopyVisuals(EntityUid uid, StampComponent otherStamp, StampComponent? stamp = null)
    {
        if (!Resolve(uid, ref stamp))
            return;

        stamp.StampedColor = otherStamp.StampedColor;
        stamp.StampedName = otherStamp.StampedName;
        stamp.StampState = otherStamp.StampState;

        _itemSys.VisualsChanged(uid);
        Dirty(uid, stamp);
    }

    // Updates chameleon visuals and meta information.
    // This function is called on a server after user selected new outfit.
    // And after that on a client after state was updated.
    // This 100% makes sure that server and client have exactly same data.
    protected void UpdateVisuals(EntityUid uid, ChameleonStampComponent component)
    {
        if (string.IsNullOrEmpty(component.Default) ||
            !_proto.TryIndex(component.Default, out EntityPrototype? proto))
            return;

        // world sprite icon
        UpdateSprite(uid, proto);

        // copy name and description, unless its an ID card
        if (!HasComp<IdCardComponent>(uid))
        {
            var meta = MetaData(uid);
            _metaData.SetEntityName(uid, proto.Name, meta);
            _metaData.SetEntityDescription(uid, proto.Description, meta);
        }

        // item sprite logic
        if (TryComp(uid, out ItemComponent? item) &&
            proto.TryGetComponent(out ItemComponent? otherItem, _factory))
        {
            _itemSystem.CopyVisuals(uid, otherItem, item);
        }

        // clothing sprite logic
        if (TryComp(uid, out StampComponent? stamp) &&
            proto.TryGetComponent("Stamp", out StampComponent? otherStamp))
        {
            CopyVisuals(uid, otherStamp, stamp);
        }
    }

    protected virtual void UpdateSprite(EntityUid uid, EntityPrototype proto) { }

    /// <summary>
    ///     Check if this entity prototype is valid target for chameleon item.
    /// </summary>
    public bool IsValidTarget(EntityPrototype proto)
    {
        // check if entity is valid
        if (proto.Abstract || proto.HideSpawnMenu)
            return false;

        // check if it is marked as valid chameleon target
        if (!proto.TryGetComponent(out TagComponent? tag, _factory) || !_tag.HasTag(tag, "WhitelistChameleon"))
            return false;

        // check if it's valid clothing
        if (!proto.TryGetComponent("Stamp", out StampComponent? stamp))
            return false;

        return true;
    }
}
