using Content.Server.Chat.Systems;
using Content.Server.Radio;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Radio;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Stories.Shadowling;

public sealed partial class ShadowlingSystem
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private const string ShadowlingMindRadioPrototype = "ShadowlingMind";

    public void InitializeRadio()
    {
        SubscribeLocalEvent<ShadowlingThrallComponent, RadioReceiveEvent>(OnIntrinsicReceive);
        SubscribeLocalEvent<ShadowlingComponent, RadioReceiveEvent>(OnIntrinsicReceive);
        SubscribeLocalEvent<ShadowlingThrallComponent, EntitySpokeEvent>(OnIntrinsicSpeak);
        SubscribeLocalEvent<ShadowlingComponent, EntitySpokeEvent>(OnIntrinsicSpeak);
    }

    private void OnIntrinsicSpeak(EntityUid uid, ShadowlingComponent component, EntitySpokeEvent args)
    {
        HandleIntrinsicSpeak(uid, args);
    }

    private void OnIntrinsicSpeak(EntityUid uid, ShadowlingThrallComponent component, EntitySpokeEvent args)
    {
        HandleIntrinsicSpeak(uid, args);
    }

    private void HandleIntrinsicSpeak(EntityUid uid, EntitySpokeEvent args)
    {
        if (!_prototype.TryIndex<RadioChannelPrototype>(ShadowlingMindRadioPrototype, out _))
        {
            Log.Error("Cannot find ShadowlingMind radio prototype, specified prototype: {0};", ShadowlingMindRadioPrototype);
            return;
        }

        if (args.Channel != null && args.Channel.ID == ShadowlingMindRadioPrototype)
        {
            _radio.SendRadioMessage(uid, args.Message, args.Channel, uid);
            args.Channel = null; // prevent duplicate messages from other listeners.
        }
    }

    private void OnIntrinsicReceive(EntityUid uid, ShadowlingComponent component, ref RadioReceiveEvent args)
    {
        HandleIntrinsicReceive(uid, ref args);
    }
    private void OnIntrinsicReceive(EntityUid uid, ShadowlingThrallComponent component, ref RadioReceiveEvent args)
    {
        HandleIntrinsicReceive(uid, ref args);
    }

    private void HandleIntrinsicReceive(EntityUid uid, ref RadioReceiveEvent args)
    {
        if (TryComp(uid, out ActorComponent? actor))
            _netMan.ServerSendMessage(args.ChatMsg, actor.PlayerSession.Channel);
    }
}
