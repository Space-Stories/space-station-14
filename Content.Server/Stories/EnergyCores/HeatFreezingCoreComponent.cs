using Content.Shared.Atmos;

namespace Content.Server.Stories.EnergyCores;

[RegisterComponent]

public sealed partial class HeatFreezingCoreComponent : Component
{
    [DataField("port"), ViewVariables(VVAccess.ReadWrite)]
    public string PortName { get; set; } = "pipe";

    [DataField("filterGases")]
    public HashSet<Gas> FilterGases = new()
        {
            Gas.Frezon,
            Gas.Ammonia,
            Gas.NitrousOxide,
            Gas.Plasma
        };
    [DataField]
    public float FilterTemperature = Atmospherics.T0C + 50;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaxPressure = 3000;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float TransferRate = 100;

    [DataField]
    public Gas AbsorbGas = Gas.Frezon;
}
