using Content.Client.Message;
using Content.Shared.Stories.Sponsor.AntagSelect;

namespace Content.Client.Stories.Sponsor.UI;

public sealed class AntagSelectBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private AntagSelectMenu? _menu;

    [ViewVariables]
    public string CurrentAntag { get; private set; } = default!;

    [ViewVariables]
    public HashSet<string> Antags { get; private set; } = new();

    public AntagSelectBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new AntagSelectMenu(this);
        _menu.OnClose += Close;
        _menu.OpenCentered();
    }

    public void AntagSelected(string antag)
    {
        CurrentAntag = antag;
        SendMessage(new AntagSelectedMessage(antag));
    }

    public void PickAntag(string antag)
    {
        SendMessage(new PickAntagMessage(antag));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu == null)
            return;

        if (state is AntagSelectInterfaceState fullState)
        {
            CurrentAntag = fullState.CurrentAntag;
            Antags = fullState.Antags;
            _menu.Request.Disabled = !fullState.CanPickCurrentAntag;
            _menu.StatusLabel.SetMarkup(fullState.Status.ToMarkup());
            _menu.UpdateAntagSelect(fullState.Antags, CurrentAntag);
        }

        if (state is SelectedAntagInterfaceState antagState)
        {
            CurrentAntag = antagState.CurrentAntag;
            _menu.Request.Disabled = !antagState.CanPickCurrentAntag;
            _menu.StatusLabel.SetMarkup(antagState.Status.ToMarkup());
            _menu.UpdateAntagSelect(Antags, CurrentAntag);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        _menu?.Dispose();
    }
}

