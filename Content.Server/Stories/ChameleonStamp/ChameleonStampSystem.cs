using Content.Server.IdentityManagement;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Prototypes;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Stories.ChameleonStamp;

namespace Content.Server.Stories.ChameleonStamp;

public sealed class ChameleonStampSystem : SharedChameleonStampSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChameleonStampComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChameleonStampComponent, GetVerbsEvent<InteractionVerb>>(OnVerb);
        SubscribeLocalEvent<ChameleonStampComponent, ChameleonStampPrototypeSelectedMessage>(OnSelected);
    }

    private void OnMapInit(EntityUid uid, ChameleonStampComponent component, MapInitEvent args)
    {
        SetSelectedPrototype(uid, component.Default, true, component);
    }

    private void OnVerb(EntityUid uid, ChameleonStampComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || component.User != args.User)
            return;

        args.Verbs.Add(new InteractionVerb()
        {
            Text = Loc.GetString("chameleon-component-verb-text"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Act = () => TryOpenUi(uid, args.User, component)
        });
    }

    private void OnSelected(EntityUid uid, ChameleonStampComponent component, ChameleonStampPrototypeSelectedMessage args)
    {
        SetSelectedPrototype(uid, args.SelectedId, component: component);
    }

    private void TryOpenUi(EntityUid uid, EntityUid user, ChameleonStampComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        if (!TryComp(user, out ActorComponent? actor))
            return;
        _uiSystem.TryToggleUi(uid, ChameleonUiKey.Key, actor.PlayerSession);
    }

    private void UpdateUi(EntityUid uid, ChameleonStampComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var state = new ChameleonStampBoundUserInterfaceState(component.Slot, component.Default);
        _uiSystem.SetUiState(uid, ChameleonUiKey.Key, state);
    }

    /// <summary>
    ///     Change chameleon items name, description and sprite to mimic other entity prototype.
    /// </summary>
    public void SetSelectedPrototype(EntityUid uid, string? protoId, bool forceUpdate = false,
        ChameleonStampComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        // check that wasn't already selected
        // forceUpdate on component init ignores this check
        if (component.Default == protoId && !forceUpdate)
            return;

        // make sure that it is valid change
        if (string.IsNullOrEmpty(protoId) || !_proto.TryIndex(protoId, out EntityPrototype? proto))
            return;
        if (!IsValidTarget(proto))
            return;
        component.Default = protoId;
        UpdateVisuals(uid, component);
        UpdateUi(uid, component);
        Dirty(uid, component);
    }
}
