using Content.Shared.Actions.Events;
using Content.Shared.Abilities.SpafEvolf; 
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using System;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;

namespace Content.Server.Abilities.SpafEvolf;

public sealed class SpafEvolfSystem : SharedSpafEvolfSystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafEvolfComponent, SpafEvolfEvent>(OnSpafEvolf); // Tracking the event
    }


    private void OnSpafEvolf(EntityUid uid, SpafEvolfComponent component, SpafEvolfEvent args) // Creating an action for the evolution of an entity
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        //make sure the hunger doesn't go into the negatives
        if (hunger.CurrentHunger < component.HungerPerSpafEvolf)
        {
            _popup.PopupEntity(Loc.GetString("your-pathetic-appearance-needs-more-food"), uid, uid); // We report a shortage of food
            return;
        }

        args.Handled = true;
        _hunger.ModifyHunger(uid, -component.HungerPerSpafEvolf, hunger); //taking away food

        var child = Spawn(component.TransMobSpawnId, Transform(uid).Coordinates); // Creating a child object
        QueueDel(uid); // deleting the past body

        if (_mindSystem.TryGetMind(uid, out var mindId, out var mind))
        {
            _mindSystem.TransferTo(mindId, child, mind: mind); // moving the mind into a new body
        }

    }
}
