using Content.Client.Stylesheets;
using Content.Shared.Administration;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.Administration.UI.CustomControls;

[GenerateTypedNameReferences]
public sealed partial class PlayerListEntry : BoxContainer
{
    public PlayerListEntry()
    {
        RobustXamlLoader.Load(this);
    }

    public event Action<PlayerInfo>? OnPinStatusChanged;

    public void Setup(PlayerInfo info, Func<PlayerInfo, string, string>? overrideText)
    {
        Update(info, overrideText);
        PlayerEntryPinButton.OnPressed += HandlePinButtonPressed(info);
    }

    private Action<BaseButton.ButtonEventArgs> HandlePinButtonPressed(PlayerInfo info)
    {
        return args =>
        {
            info.IsPinned = !info.IsPinned;
            UpdatePinButtonTexture(info.IsPinned);
            OnPinStatusChanged?.Invoke(info);
        };
    }

    private void Update(PlayerInfo info, Func<PlayerInfo, string, string>? overrideText)
    {
        PlayerEntryLabel.Text = overrideText?.Invoke(info, $"{info.CharacterName} ({info.Username})") ??
                                $"{info.CharacterName} ({info.Username})";

        UpdatePinButtonTexture(info.IsPinned);
    }

    private void UpdatePinButtonTexture(bool isPinned)
    {
        if (isPinned)
        {
            PlayerEntryPinButton?.RemoveStyleClass(StyleNano.StyleClassPinButtonUnpinned);
            PlayerEntryPinButton?.AddStyleClass(StyleNano.StyleClassPinButtonPinned);
        }
        else
        {
            PlayerEntryPinButton?.RemoveStyleClass(StyleNano.StyleClassPinButtonPinned);
            PlayerEntryPinButton?.AddStyleClass(StyleNano.StyleClassPinButtonUnpinned);
        }
    }
}
