using Content.Shared.Actions.Events;
using Content.Shared.Abilities.SpafStealth;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.Abilities.SpafStealth;

public sealed class SpafStealthSystem : SharedSpafStealthSystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpafStealthComponent, SpafStealthEvent>(OnSpafStealth);   // Tracking the event
    }


    private void OnSpafStealth(EntityUid uid, SpafStealthComponent component, SpafStealthEvent args) // Creating an action to switch an entity to a temporary invisibility mode
    {

        if (args.Handled)
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        //make sure the hunger doesn't go into the negatives
        if (hunger.CurrentHunger < component.HungerPerSpafStealth)
        {
            _popup.PopupEntity(Loc.GetString("your-pathetic-appearance-needs-more-food"), uid, uid); // We report a shortage of food
            return;
        }

        args.Handled = true;
        _hunger.ModifyHunger(uid, -component.HungerPerSpafStealth, hunger); // Taking away food

        var stealth = EnsureComp<StealthComponent>(uid);
        var visibility = -0.8f; // Setting the invisibility level 
        _stealth.SetVisibility(uid, visibility, stealth);

        // We set the timer for 5 seconds in order to remove the invisibility later
        Task.Delay(5 * 1000).ContinueWith(_ =>
        {
            var stealth = RemComp<StealthComponent>(uid);
        });
    }
}
