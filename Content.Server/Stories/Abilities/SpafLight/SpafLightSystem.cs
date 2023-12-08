using Content.Shared.Actions.Events;
using Content.Shared.Abilities.SpafLight; 
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Server.Popups;
using Robust.Server.GameObjects;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Server.Light.EntitySystems;
using Content.Server.Light.Events;
using Content.Shared.Light.Components;
using Content.Server.Light.EntitySystems;
using System;


namespace Content.Server.Abilities.SpafLight;

public sealed class SpafLightSystem : SharedSpafLightSystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafLightComponent, SpafLightEvent>(OnSpafLight); // Tracking the event
    }



    bool lighton = false; // This variable is needed to track the status of the button at the moment

    private void OnSpafLight(EntityUid uid, SpafLightComponent component, SpafLightEvent args) // Creating an action to turn on/off the flashlight at the entity
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        //make sure the hunger doesn't go into the negatives
        if (hunger.CurrentHunger < component.HungerPerSpafLight)
        {
            _popup.PopupEntity(Loc.GetString("your-pathetic-appearance-needs-more-food"), uid, uid);  // We report a shortage of food
            return;
        }

        args.Handled = true;
        _hunger.ModifyHunger(uid, -component.HungerPerSpafLight, hunger); // Taking away food

        var radius = 3.0f;    //
        var energy = 2.0f;    // We set the characteristics of the lamp that turns on
        var softness = 1.0f;  //

        if (_pointLight.TryGetLight(uid, out var pointLightComponent) && lighton == false)
        {
            _pointLight.SetEnabled(uid, true, pointLightComponent);
            _pointLight.SetRadius(uid, (float) radius, pointLightComponent);
            _pointLight.SetEnergy(uid, (float) energy, pointLightComponent);
            _pointLight.SetSoftness(uid, (float) softness, pointLightComponent);
            lighton = true;
        }
        else if(lighton == true)
        {
            var radius2 = 0.0f;     //
            var energy2 = 0.0f;     //  We set the characteristics of the lamp that turns off
            var softness2 = 0.0f;   //
            _pointLight.SetEnabled(uid, true, pointLightComponent);
            _pointLight.SetRadius(uid, (float) radius2, pointLightComponent);
            _pointLight.SetEnergy(uid, (float) energy2, pointLightComponent);
            _pointLight.SetSoftness(uid, (float) softness2, pointLightComponent);
            lighton = false;
        }
    }
}
