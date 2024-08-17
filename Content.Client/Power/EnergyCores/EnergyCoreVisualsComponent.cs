

namespace Content.Client.Power.EnergyCores;

[RegisterComponent]
public sealed partial class EnergyCoreVisualsComponent : Component
{
    [DataField(required: true)]
    public string OnState = default!;

    [DataField(required: true)]
    public string OffState = default!;


}

