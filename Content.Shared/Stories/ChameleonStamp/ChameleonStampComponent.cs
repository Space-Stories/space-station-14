using Content.Shared.Stories.ChameleonStamp;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Stories.ChameleonStamp;

/// <summary>
///     Allow players to change clothing sprite to any other clothing prototype.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedChameleonStampSystem))]
public sealed partial class ChameleonStampComponent : Component
{
    /// <summary>
    ///     EntityPrototype id that chameleon item is trying to mimic.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId? Default;

    /// <summary>
    ///     Current user that wears chameleon clothing.
    /// </summary>
    [ViewVariables]
    public EntityUid? User;
}

[Serializable, NetSerializable]
public sealed class ChameleonStampBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly string? SelectedId;

    public ChameleonStampBoundUserInterfaceState(string? selectedId)
    {
        SelectedId = selectedId;
    }
}

[Serializable, NetSerializable]
public sealed class ChameleonStampPrototypeSelectedMessage : BoundUserInterfaceMessage
{
    public readonly string SelectedId;

    public ChameleonStampPrototypeSelectedMessage(string selectedId)
    {
        SelectedId = selectedId;
    }
}

[Serializable, NetSerializable]
public enum ChameleonUiKey : byte
{
    Key
}
