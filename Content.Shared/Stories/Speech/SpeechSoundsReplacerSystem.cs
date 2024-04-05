using Content.Shared.Inventory.Events;
using Content.Shared.Speech;

namespace Content.Shared.Stories.Speech;

public sealed class SpeechSoundsReplacerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeechSoundsReplacerComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<SpeechSoundsReplacerComponent, GotUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, SpeechSoundsReplacerComponent component, GotEquippedEvent args)
    {
        if (EntityManager.TryGetComponent<SpeechComponent>(args.Equipee, out var speech))
        {
            component.PreviousSound = speech.SpeechSounds;
            speech.SpeechSounds = component.SpeechSounds;
        }
    }

    private void OnUnequip(EntityUid uid, SpeechSoundsReplacerComponent component, GotUnequippedEvent args)
    {
        if (EntityManager.TryGetComponent<SpeechComponent>(args.Equipee, out var speech))
        {
            speech.SpeechSounds = component.PreviousSound;
            component.PreviousSound = null;
        }
    }
}
