using Content.Server.Actions;
using Content.Server.Stories.Photosensitivity;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Stories.Shadowling;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Utility;

namespace Content.Server.Stories.Shadowling;
public sealed partial class ShadowlingSystem : SharedShadowlingSystem<ShadowlingThrallComponent, ShadowlingComponent>
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PhotosensitivitySystem _photosensitivity = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private const float UpdateTimer = 2f;
    private float _timer;

    public readonly List<string> AscendedAbilities = new()
    {
        "ActionShadowlingHypnosis",
        "ActionShadowlingGlare",
        "ActionShadowlingVeil",
        // "ActionShadowlingPlaneShift",
        "ActionShadowlingShadowWalk",
        "ActionShadowlingIcyVeins",
        "ActionShadowlingCollectiveMind",
        "ActionShadowlingRapidReHatch",
        "ActionShadowlingBlindnessSmoke",
        // "ActionShadowlingDrainThralls",
        "ActionShadowlingSonicScreech",
        "ActionShadowlingBlackRecuperation",
        "ActionShadowlingLightningStorm",
        "ActionShadowlingAnnihilate",
    };

    public override void Initialize()
    {
        base.Initialize();
        InitializeBase();
        InitializeRadio();
        InitializeThralls();
    }

    private void InitializeBase()
    {
        SubscribeLocalEvent<ShadowlingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowlingComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ShadowlingComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    public const string ShadowlingEnthrallAction = "ActionShadowlingEnthrall";
    public const string ShadowlingHatchAction = "ActionShadowlingHatch";

    private void OnStartup(EntityUid uid, ShadowlingComponent component, ComponentStartup args)
    {
        if (!component.Ascended)
        {
            AddAction(uid, ShadowlingEnthrallAction, component);
            AddAction(uid, ShadowlingHatchAction, component);
        }
        else
        {
            foreach (var action in AscendedAbilities)
            {
                AddAction(uid, action, component);
            }
        }

        EnsureComp<ShadowlingRoleComponent>(uid);
    }

    /// <summary>
    /// Internal method for abilities providing
    /// </summary>
    public void AddAction(EntityUid uid, string actionId, ShadowlingComponent? shadowling = null)
    {
        if (!Resolve(uid, ref shadowling))
            return;

        if (!TryComp<ActionsComponent>(uid, out var action))
            return;

        DebugTools.Assert(!shadowling.GrantedActions.ContainsKey(actionId));

        EntityUid? actionEntity = null;
        _actions.AddAction(uid, ref actionEntity, actionId, uid, action);

        if (actionEntity == null)
            return;

        shadowling.GrantedActions.Add(actionId, actionEntity.Value);
    }

    /// <summary>
    /// Internal method for abilities providing
    /// </summary>
    public void RemoveAction(EntityUid uid, string actionId, ShadowlingComponent? shadowling = null)
    {
        if (!Resolve(uid, ref shadowling))
            return;

        if (!TryComp<ActionsComponent>(uid, out var action))
            return;

        DebugTools.Assert(shadowling.GrantedActions.ContainsKey(actionId));

        var actionEntity = shadowling.GrantedActions[actionId];
        _actions.RemoveAction(uid, actionEntity, action);
        shadowling.GrantedActions.Remove(actionId);
    }

    private void OnShutdown(EntityUid uid, ShadowlingComponent component, ComponentShutdown args)
    {
        // TODO: change metabolizm back to normal
    }

    public List<EntityUid> GetEntitiesAroundShadowling<TFilter>(EntityUid uid, float radius, bool filterThralls = true) where TFilter : IComponent
    {
        List<EntityUid> result = new();

        if (!TryComp<TransformComponent>(uid, out var transform))
            return result;

        foreach (var entity in _entityLookup.GetEntitiesInRange(transform.Coordinates, radius))
        {
            if (!TryComp<TFilter>(entity, out _))
                continue;
            if (filterThralls && TryComp<ShadowlingComponent>(entity, out _))
                continue;

            result.Add(entity);
        }

        return result;
    }

    private void OnShotAttempted(EntityUid uid, ShadowlingComponent comp, ref ShotAttemptedEvent args)
    {
        _popup.PopupEntity(Loc.GetString("gun-disabled"), uid, uid);
        args.Cancel();
    }

    public override void Update(float frameTime)
    {
        _timer += frameTime;

        if (_timer < UpdateTimer)
            return;

        _timer -= UpdateTimer;

        var shadowlings = EntityQueryEnumerator<ShadowlingComponent>();

        while (shadowlings.MoveNext(out var uid, out var shadowling))
        {
            if (!shadowling.PerformLightDamage)
                continue;

            var illumination = Math.Min(_photosensitivity.GetIllumination(uid), 10);

            if (illumination > 1.5)
            {
                _damageable.TryChangeDamage(uid, shadowling.LightnessDamage * illumination, true, false);
                _popup.PopupEntity("Свет выжигает вас!", uid, uid);
            }

            if (illumination < 1)
            {
                _damageable.TryChangeDamage(uid, shadowling.DarknessHealing, true, false);
            }
        }
    }
}
