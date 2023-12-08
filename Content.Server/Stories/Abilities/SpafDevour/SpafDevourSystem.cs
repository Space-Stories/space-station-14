using Content.Shared.Actions.Events;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Server.Humanoid;
using Content.Shared.Interaction;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.SpafDevour;
using Content.Shared.SpafDevour.Components;
using Content.Shared.Humanoid;


namespace Content.Server.SpafDevour;

public sealed class SpafDevourSystem : SharedSpafDevourSystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpafDevourerComponent, SpafDevourDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(EntityUid uid, SpafDevourerComponent component, SpafDevourDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var ichorInjection = new Solution(component.Chemical, component.HealRate);

        if (component.FoodPreference == FoodPreference.All ||
            (component.FoodPreference == FoodPreference.Humanoid && HasComp<HumanoidAppearanceComponent>(args.Args.Target))) // List of allowed things in the prototype entity
        {
            ichorInjection.ScaleSolution(0.5f);

            if (component.ShouldStoreDevoured && args.Args.Target is not null)
            {
                component.Stomach.Insert(args.Args.Target.Value);
            }
            if (!TryComp<HungerComponent>(uid, out var hunger))
                return;

            float hung = 50.0f; // Why 50 ? I do not know ¯\_(ツ)_ /¯. Just people weigh +-60 kg.Dwarves, in my opinion, +-40 kg.Average weight 50. In general, this figure brings us closer to reality

            _bloodstreamSystem.TryAddToChemicals(uid, ichorInjection); // Why ichor? It's just that he will be 100% sick
            _hunger.ModifyHunger(uid, hung, hunger);
            _popup.PopupEntity(Loc.GetString("do-you-feel-a-strong-increase-in-food-in-your-stomach"), uid, uid);
        }

        else if (args.Args.Target != null)
        {
            if (!TryComp<HungerComponent>(uid, out var hunger))
                return;

            QueueDel(args.Args.Target.Value);
            float hung = 20.0f;
            _hunger.ModifyHunger(uid, hung, hunger);
            _popup.PopupEntity(Loc.GetString("do-you-feel-an-increase-in-food-in-your-stomach"), uid, uid);
        }

    }
}

