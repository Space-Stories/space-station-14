using Content.Shared.Actions.Events;
using Content.Shared.Abilities.SpafMine;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using System;

namespace Content.Server.Abilities.SpafMine;

public sealed class SpafMineSystem : SharedSpafMineSystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafMineComponent, SpafMineEvent>(OnSpafStealth);  // Tracking the event
    }


    private void OnSpafStealth(EntityUid uid, SpafMineComponent component, SpafMineEvent args) // Creating an action to create a trap object
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        //make sure the hunger doesn't go into the negatives
        if (hunger.CurrentHunger < component.HungerPerSpafMine)
        {
            _popup.PopupEntity(Loc.GetString("your-pathetic-appearance-needs-more-food"), uid, uid);  // We report a shortage of food
            return;
        }

        args.Handled = true;
        _hunger.ModifyHunger(uid, -component.HungerPerSpafMine, hunger); // Taking away food

        var child = Spawn(component.TransMobSpawnId, Transform(uid).Coordinates);  // Creating a child object
    }
}
