using Content.Shared.Stories.Lib.Invisibility;
using Robust.Client.GameObjects;

namespace Content.Client.Stories.Lib.Invisibility;

public sealed class InvisibilitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InvisibleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<InvisibleComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, InvisibleComponent invisible, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
        {
            Log.Error("Tried to apply invisible on an entity which doesn't have sprite");
            return;
        }

        sprite.Visible = false;
    }

    private void OnShutdown(EntityUid uid, InvisibleComponent invisibleComponent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.Visible = true;
    }
}
