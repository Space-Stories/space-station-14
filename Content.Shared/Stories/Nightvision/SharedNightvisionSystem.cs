using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
namespace Content.Shared.Stories.Nightvision;

public sealed class SharedNightvisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NightvisionComponent, ToggleNightvisionEvent>(OnToggle);
    }
    private void OnToggle(EntityUid uid, NightvisionComponent component, ToggleNightvisionEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;
        component.Enabled = !component.Enabled;
        if (component.Enabled && component.ToggleOnSound != null)
            _audio.PlayLocal(component.ToggleOnSound, uid, uid);
    }
}
