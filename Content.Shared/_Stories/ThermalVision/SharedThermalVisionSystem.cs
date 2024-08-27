namespace Content.Shared._Stories.ThermalVision;

public abstract class SharedThermalVisionSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ThermalVisionComponent, MapInitEvent>(OnThermalVisionMapInit);
        SubscribeLocalEvent<ThermalVisionComponent, ComponentRemove>(OnThermalVisionRemove);

        SubscribeLocalEvent<ThermalVisionComponent, AfterAutoHandleStateEvent>(OnThermalVisionAfterHandle);
    }

    private void OnThermalVisionAfterHandle(Entity<ThermalVisionComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ThermalVisionChanged(ent);
    }

    private void OnThermalVisionMapInit(Entity<ThermalVisionComponent> ent, ref MapInitEvent args)
    {
        ThermalVisionChanged(ent);
    }

    private void OnThermalVisionRemove(Entity<ThermalVisionComponent> ent, ref ComponentRemove args)
    {
        ThermalVisionRemoved(ent);
    }

    protected virtual void ThermalVisionChanged(Entity<ThermalVisionComponent> ent)
    {
    }

    protected virtual void ThermalVisionRemoved(Entity<ThermalVisionComponent> ent)
    {
    }
}
