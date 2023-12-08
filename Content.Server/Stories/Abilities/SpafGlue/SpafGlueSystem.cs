using Content.Shared.Actions.Events;
using Content.Shared.Abilities.SpafGlue;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using System;

namespace Content.Server.Abilities.SpafGlue;

public sealed class SpafGlueSystem : SharedSpafGlueSystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafGlueComponent, SpafGlueEvent>(OnSpafGlue);  // Tracking the event
    }


    private void OnSpafGlue(EntityUid uid, SpafGlueComponent component, SpafGlueEvent args) // creating an action to highlight a certain slime essence
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        //make sure the hunger doesn't go into the negatives
        if (hunger.CurrentHunger < component.HungerPerSpafGlue)
        {
            _popup.PopupEntity(Loc.GetString("your-pathetic-appearance-needs-more-food"), uid, uid); // We report a shortage of food
            return;
        }

        args.Handled = true;
        _hunger.ModifyHunger(uid, -component.HungerPerSpafGlue, hunger); // Taking away food

        var child = Spawn(component.TransMobSpawnId, Transform(uid).Coordinates); // Creating a child object
    }
}
