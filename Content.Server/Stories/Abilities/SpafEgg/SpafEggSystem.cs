using Content.Shared.Actions.Events; 
using Content.Shared.Abilities.SpafEgg; 
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using System;

namespace Content.Server.Abilities.SpafEgg;

public sealed class SpafEggSystem : SharedSpafEggSystem // Creating a system for the operation of the button
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafEggComponent, SpafEggEvent>(OnSpafEgg); // Tracking events
    }


    private void OnSpafEgg(EntityUid uid, SpafEggComponent component, SpafEggEvent args) // Creating an action that the object will perform after registering the event. In this case, we create an egg
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        // Make sure the hunger doesn't go into the negatives
        if (hunger.CurrentHunger < component.HungerPerSpafEgg)
        {
            _popup.PopupEntity(Loc.GetString("your-pathetic-appearance-needs-more-food"), uid, uid); // We inform you that there is not enough food to perform the action
            return;
        }
        args.Handled = true;
        _hunger.ModifyHunger(uid, -component.HungerPerSpafEgg, hunger); // Taking away food

        var child = Spawn(component.TransMobSpawnId, Transform(uid).Coordinates); // Creating an egg. It will automatically create a guest role, but there is no need to prescribe it here
    }
}
