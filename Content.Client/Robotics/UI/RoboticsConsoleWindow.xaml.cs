using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Lock;
using Content.Shared.Robotics;
using Content.Shared.Robotics.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Robotics.UI;

[GenerateTypedNameReferences]
public sealed partial class RoboticsConsoleWindow : FancyWindow
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly LockSystem _lock;
    private readonly SpriteSystem _sprite;

    public Action<string>? OnDisablePressed;
    public Action<string>? OnDestroyPressed;

    private Entity<RoboticsConsoleComponent, LockComponent?> _console;
    private string? _selected;
    private Dictionary<string, CyborgControlData> _cyborgs = new();

    public RoboticsConsoleWindow(EntityUid console)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _lock = _entMan.System<LockSystem>();
        _sprite = _entMan.System<SpriteSystem>();

        _console = (console, _entMan.GetComponent<RoboticsConsoleComponent>(console), null);
        _entMan.TryGetComponent(_console, out _console.Comp2);

        Cyborgs.OnItemSelected += args =>
        {
            if (Cyborgs[args.ItemIndex].Metadata is not string address)
                return;

            _selected = address;
            PopulateData();
        };
        Cyborgs.OnItemDeselected += _ =>
        {
            _selected = null;
            PopulateData();
        };

        // these won't throw since buttons are only visible if a borg is selected
        DisableButton.OnPressed += _ =>
        {
            OnDisablePressed?.Invoke(_selected!);
        };
        DestroyButton.OnPressed += _ =>
        {
            OnDestroyPressed?.Invoke(_selected!);
        };

        // cant put multiple styles in xaml for some reason
        DestroyButton.StyleClasses.Add(StyleBase.ButtonCaution);
    }

    public void UpdateState(RoboticsConsoleState state)
    {
        _cyborgs = state.Cyborgs;

        // clear invalid selection
        if (_selected is {} selected && !_cyborgs.ContainsKey(selected))
            _selected = null;

        var hasCyborgs = _cyborgs.Count > 0;
        NoCyborgs.Visible = !hasCyborgs;
        CyborgsContainer.Visible = hasCyborgs;
        PopulateCyborgs();

        PopulateData();

        var locked = _lock.IsLocked((_console, _console.Comp2));
        DangerZone.Visible = !locked;
        LockedMessage.Visible = locked;
    }

    private void PopulateCyborgs()
    {
        // _selected might get set to null when recreating so copy it first
        var selected = _selected;
        Cyborgs.Clear();
        foreach (var (address, data) in _cyborgs)
        {
            var item = Cyborgs.AddItem(data.Name, _sprite.Frame0(data.ChassisSprite!), metadata: address);
            item.Selected = address == selected;
        }
        _selected = selected;
    }

    private void PopulateData()
    {
        if (_selected is not {} selected)
        {
            SelectCyborg.Visible = true;
            BorgContainer.Visible = false;
            return;
        }

        SelectCyborg.Visible = false;
        BorgContainer.Visible = true;

        var data = _cyborgs[selected];
        var model = data.ChassisName;

        BorgSprite.Texture = _sprite.Frame0(data.ChassisSprite!);

        var batteryColor = data.Charge switch {
            < 0.2f => "red",
            < 0.4f => "orange",
            < 0.6f => "yellow",
            < 0.8f => "green",
            _ => "blue"
        };

        var text = new FormattedMessage();
        text.PushMarkup(Loc.GetString("robotics-console-model", ("name", model)));
        text.AddMarkup(Loc.GetString("robotics-console-designation"));
        text.AddText($" {data.Name}\n"); // prevent players trolling by naming borg [color=red]satan[/color]
        text.PushMarkup(Loc.GetString("robotics-console-battery", ("charge", (int) (data.Charge * 100f)), ("color", batteryColor)));
        text.PushMarkup(Loc.GetString("robotics-console-brain", ("brain", data.HasBrain)));
        text.AddMarkup(Loc.GetString("robotics-console-modules", ("count", data.ModuleCount)));
        BorgInfo.SetMessage(text);

        // how the turntables
        DisableButton.Disabled = !(data.HasBrain && data.CanDisable);
        DestroyButton.Disabled = _timing.CurTime < _console.Comp1.NextDestroy;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        DestroyButton.Disabled = _timing.CurTime < _console.Comp1.NextDestroy;
    }
}
