using Content.Shared.Actions.Events;
using Content.Shared.Abilities.SpafFood;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using System;

namespace Content.Server.Abilities.SpafFood;

public sealed class SpafFoodSystem : SharedSpafFoodSystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafFoodComponent, SpafFoodEvent>(OnSpafFood); // Tracking events
    }


    private void OnSpafFood(EntityUid uid, SpafFoodComponent component, SpafFoodEvent args) // creating an action so that a person can find out their amount of food
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        float myFloat = hunger.CurrentHunger;
        string myString = myFloat.ToString();
        _popup.PopupEntity(Loc.GetString(myString), uid, uid); // We display the amount of food on the screen
    }
}
