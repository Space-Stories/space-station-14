using Content.Client.Clothing.Systems;
using Content.Shared.Clothing.Components;
using Content.Client.Clothing.UI;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Content.Client.Stories.ChameleonStamp;
using Content.Shared.Stories.ChameleonStamp;
namespace Content.Client.Stories.ChameleonStamp.UI;

[UsedImplicitly]
public sealed class ChameleonStampBoundUserInterface : BoundUserInterface
{
    private readonly ChameleonStampSystem _chameleon;

    [ViewVariables]
    private ChameleonMenu? _menu;

    public ChameleonStampBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _chameleon = EntMan.System<ChameleonStampSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ChameleonMenu>();
        _menu.OnIdSelected += OnIdSelected;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not ChameleonStampBoundUserInterfaceState st)
        {
            Logger.Info($"Проверка стейтов не пройдена.");
            return;
        }   
        Logger.Info($"Проверка стейтов пройдена.");
        var targets = _chameleon.GetValidTargets(st.Slot);
        _menu?.UpdateState(targets, st.SelectedId);
    }

    private void OnIdSelected(string selectedId)
    {
        SendMessage(new ChameleonStampPrototypeSelectedMessage(selectedId));
    }
}
