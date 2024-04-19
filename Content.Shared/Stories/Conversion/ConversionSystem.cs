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
using Content.Shared.Rounding;

namespace Content.Shared.SpaceStories.Conversion;

public abstract class SharedConversionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _sharedStun = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly ISerializationManager _seriMan = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ConversionableComponent, MindShieldImplantedEvent>(OnMindShield);
    }
    private void OnMindShield(EntityUid uid, ConversionableComponent component, ref MindShieldImplantedEvent args)
    {
        HashSet<string> toRevert = new();
        foreach (var (key, conversion) in component.ActiveConversions)
        {
            if (conversion.RevertOnMindShield)
                toRevert.Add(key);
        }
        foreach (var id in toRevert)
        {
            TryRevert(uid, id);
        }
    }
    public bool IsConverted(EntityUid uid, string id, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        if (!component.ActiveConversions.TryGetValue(id, out var proto))
            return false;
        return true;
    }
    public bool CanConvert(EntityUid uid, string id, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        if (!_proto.TryIndex<ConversionPrototype>(id, out var proto))
            return false;
        if (component.ActiveConversions.ContainsKey(id))
            return false;
        if (!component.AllowedConversions.Contains(id))
            return false;
        if (proto.NeedMind && !_mind.TryGetMind(uid, out var mindId, out var mind))
            return false;
        if (proto.RevertOnMindShield && HasComp<MindShieldComponent>(uid))
            return false;

        return true;
    }
    public bool CanRevert(EntityUid uid, string id, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        if (!_proto.TryIndex<ConversionPrototype>(id, out var proto))
            return false;
        if (!component.ActiveConversions.ContainsKey(id))
            return false;

        return true;
    }
    public bool TryConvert(EntityUid uid, string id, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (!CanConvert(uid, id))
            return false;

        Convert(uid, id);

        return true;
    }
    public bool TryRevert(EntityUid uid, string id, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (!CanRevert(uid, id))
            return false;

        Revert(uid, id);

        return true;
    }
    public void Convert(EntityUid uid, string id, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var proto = _proto.Index<ConversionPrototype>(id);

        component.ActiveConversions.Add(id, proto);
        _audioSystem.PlayGlobal(proto.SoundOnConvert, uid);
        // * По логике тут нужно отправлять сообщение игроку, но в Shared это недоступно, но это реазизованно через ивент.
        RaiseLocalEvent(uid, new ConvertedEvent(uid, id, proto));

        proto.ComponentsOnConvert?.ApplyTo(uid);
        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            proto.RolesOnConvert?.ApplyTo(mindId, mind);
    }
    public void Revert(EntityUid uid, string id, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.ActiveConversions.TryGetValue(id, out var proto))
            return;

        _audioSystem.PlayGlobal(proto.SoundOnRevert, uid);
        component.ActiveConversions.Remove(id);
        // * По логике тут нужно отправлять сообщение игроку, но в Shared это недоступно, но это реазизованно через ивент.
        RaiseLocalEvent(uid, new RevertedEvent(uid, id, proto));

        proto.ComponentsOnRevert?.ApplyTo(uid);
        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            proto.RolesOnRevert?.ApplyTo(mindId, mind);
    }
}

public sealed class ConvertedEvent(EntityUid uid, string id, ConversionPrototype prototype) : HandledEntityEventArgs
{
    public readonly EntityUid Uid = uid;
    public string Id = id;
    public ConversionPrototype Prototype = prototype;
}

public sealed class RevertedEvent(EntityUid uid, string id, ConversionPrototype prototype) : HandledEntityEventArgs
{
    public readonly EntityUid Uid = uid;
    public string Id = id;
    public ConversionPrototype Prototype = prototype;
}
