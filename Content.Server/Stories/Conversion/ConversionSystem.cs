using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.SpaceStories.Empire.Components;
using Content.Shared.Stunnable;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Roles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared.Stories.Mindshield;
using Content.Shared.SpaceStories.Conversion;
using Robust.Shared.Prototypes;
using Content.Shared.Mindshield.Components;
using Content.Shared.Actions;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.SpaceStories.Force.LightSaber;
using Content.Shared.Alert;
using Robust.Shared.Serialization.Manager;
using Content.Shared.SpaceStories.Force;
using Content.Server.Chat.Managers;
using Content.Server.Radio.Components;
using System.Linq;

namespace Content.Server.SpaceStories.Conversion;

public sealed class ConversionSystem : SharedConversionSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ConversionableComponent, ConvertedEvent>(OnConvert);
        SubscribeLocalEvent<ConversionableComponent, RevertedEvent>(OnRevert);
    }
    private void OnRevert(EntityUid uid, ConversionableComponent component, RevertedEvent args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;
        if (args.Prototype.GoodbyeMessage != null && mind.Session != null && args.Prototype.NeedMind)
        {
            var message = Loc.GetString(args.Prototype.GoodbyeMessage);
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
            _chatManager.ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, mind.Session.Channel, Color.Red);
        }

        EnsureComp<IntrinsicRadioReceiverComponent>(uid);
        EnsureComp<IntrinsicRadioTransmitterComponent>(uid).Channels.ExceptWith(args.Prototype.Channels);
        EnsureComp<ActiveRadioComponent>(uid).Channels.ExceptWith(args.Prototype.Channels);

        _stun.TryParalyze(uid, TimeSpan.FromSeconds(3), true);
    }
    private void OnConvert(EntityUid uid, ConversionableComponent component, ConvertedEvent args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;
        if (args.Prototype.WelcomeMessage != null && mind.Session != null && args.Prototype.NeedMind)
        {
            var message = Loc.GetString(args.Prototype.WelcomeMessage);
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
            _chatManager.ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, mind.Session.Channel, Color.Red);
        }

        EnsureComp<IntrinsicRadioReceiverComponent>(uid);
        EnsureComp<IntrinsicRadioTransmitterComponent>(uid).Channels.UnionWith(args.Prototype.Channels);
        EnsureComp<ActiveRadioComponent>(uid).Channels.UnionWith(args.Prototype.Channels);
    }
}
