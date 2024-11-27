using Content.Shared.Cuffs.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Stories.ForceUser.Actions.Events;
using Content.Server.Store.Components;
using Content.Shared.Stories.ForceUser;
using Robust.Shared.Physics.Components;
using Content.Shared.Physics;
using Content.Shared.Mobs;
using Content.Shared.Stories.Empire.Components;
using Content.Server.Stories.ForceUser.ProtectiveBubble.Components;
using Content.Shared.Store.Components;

namespace Content.Server.Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeSimpleActions()
    {
        SubscribeLocalEvent<HypnosisTargetActionEvent>(OnHypnosis); // FIXME: Тут не должно быть этого - start
        SubscribeLocalEvent<ForceUserComponent, FrozeBulletsActionEvent>(OnFrozeBullets);
        SubscribeLocalEvent<ForceUserComponent, ForceShopActionEvent>(OnShop);
        SubscribeLocalEvent<ForceUserComponent, ForceLookUpActionEvent>(OnLookUp); // FIXME: Тут не должно быть этого - end
    }
    private void OnLookUp(EntityUid uid, ForceUserComponent component, ForceLookUpActionEvent args)
    {
        if (args.Handled) return;
        var ents = _lookup.GetEntitiesInRange<ForceUserComponent>(_xform.GetMapCoordinates(uid), args.Range);
        foreach (var ent in ents)
        {
            if (ents.Count == 1 && ent.Owner == uid)
            {
                _popup.PopupEntity(Loc.GetString("force-lookup-lonely"), uid, uid);
            }
            else if (ent.Owner == uid) continue;
            else if (ents.Count == 2) _popup.PopupEntity(Loc.GetString("force-lookup-one", ("name", ent.Comp.Name())), uid, uid);
            else if (ents.Count > 2) _popup.PopupEntity(Loc.GetString("force-lookup-many"), uid, uid);
            break;
        }
        args.Handled = true;
    }
    private void OnFrozeBullets(EntityUid uid, ForceUserComponent component, FrozeBulletsActionEvent args)
    {
        if (args.Handled) return;
        _statusEffect.TryAddStatusEffect(uid, "FrozeBullets", TimeSpan.FromSeconds(args.Seconds), true, "FrozeBullets");
        args.Handled = true;
    }
    private void OnShop(EntityUid uid, ForceUserComponent component, ForceShopActionEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;
        _store.ToggleUi(uid, uid, store);
    }
    private void OnHypnosis(HypnosisTargetActionEvent args)
    {
        if (args.Handled || _mobState.IsIncapacitated(args.Target) || HasComp<MindShieldComponent>(args.Target)) return;
        _conversion.TryConvert(args.Target, "HypnotizedEmpire", args.Performer); // FIXME: Hardcode. Исправим в обновлении инквизитора.
        args.Handled = true;
    }
}
