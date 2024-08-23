using Content.Shared.Stories.EnergyCores;
using Robust.Client.GameObjects;
using Content.Client.Power;

namespace Content.Client.Stories.EnergyCores;

public sealed partial class EnergyCoreSystem : VisualizerSystem<EnergyCoreVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, EnergyCoreVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        if (AppearanceSystem.TryGetData<EnergyCoreState>(uid, EnergyCoreVisualLayers.IsOn, out var res, args.Component))
        {
            args.Sprite.LayerSetVisible(PowerDeviceVisualLayers.Powered, false);
            foreach (var cur in args.Sprite.AllLayers)
            {
                cur.Visible = false;
            }
            switch (res)
            {
                case EnergyCoreState.Enabled: args.Sprite.LayerSetVisible(EnergyCoreVisualLayers.IsOn, true); break;
                case EnergyCoreState.Disabled: args.Sprite.LayerSetVisible(EnergyCoreVisualLayers.IsOff, true); break;
                case EnergyCoreState.Enabling: args.Sprite.LayerSetVisible(EnergyCoreVisualLayers.Enabling, true); break;
                case EnergyCoreState.Disabling: args.Sprite.LayerSetVisible(EnergyCoreVisualLayers.Disabling, true); break;
                default: Logger.Error("Incorrect state by " + uid); break;
            }
        }
    }
}
