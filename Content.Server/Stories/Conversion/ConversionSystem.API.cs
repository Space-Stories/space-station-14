using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Stories.Conversion;
using Robust.Shared.Prototypes;
using Content.Server.Radio.Components;
using Content.Shared.Database;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Stories.Conversion;

public sealed partial class ConversionSystem
{
    public HashSet<EntityUid> GetEntitiesConvertedBy(EntityUid? uid, ProtoId<ConversionPrototype> prototype)
    {
        HashSet<EntityUid> entities = new();
        var query = AllEntityQuery<ConversionableComponent>();

        while (query.MoveNext(out var entity, out var comp))
        {
            foreach (var conversion in comp.ActiveConversions)
            {
                if (conversion.Key == prototype.Id && GetEntity(conversion.Value.Owner) == uid)
                    entities.Add(entity);
            }
        }

        return entities;
    }
    public bool TryGetConversion(EntityUid uid, [NotNullWhen(true)] out ConversionData? data, ProtoId<ConversionPrototype> prototype, ConversionableComponent? component = null)
    {
        if (Resolve(uid, ref component) && component.ActiveConversions.TryGetValue(prototype.Id, out var conversion))
        {
            data = conversion;
            return true;
        }
        data = null;
        return false;
    }
    public bool TryRevert(EntityUid target, ProtoId<ConversionPrototype> prototype, EntityUid? performer = null, ConversionableComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!_prototype.TryIndex(prototype, out var proto))
            return false;

        if (!CanRevert(target, prototype, performer, component))
            return false;

        Revert(target, proto, performer, component);
        return true;
    }
    public void Revert(EntityUid target, ConversionPrototype proto, EntityUid? performer = null, ConversionableComponent? component = null)
    {
        if (!Resolve(target, ref component))
            return;

        if (!component.ActiveConversions.TryGetValue(proto.ID, out var data))
            return;

        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return;

        if (proto.EndBriefing != null)
            _antag.SendBriefing(target, Loc.GetString(proto.EndBriefing.Value.Text ?? ""), proto.EndBriefing.Value.Color, proto.EndBriefing.Value.Sound);

        EntityManager.RemoveComponents(target, registry: proto.Components);
        MindRemoveRoles(mindId, proto.MindComponents);

        if (proto.Channels.Count > 0)
        {
            EnsureComp<IntrinsicRadioReceiverComponent>(target);
            EnsureComp<IntrinsicRadioTransmitterComponent>(target).Channels.ExceptWith(proto.Channels);
            EnsureComp<ActiveRadioComponent>(target).Channels.ExceptWith(proto.Channels);
        }

        component.ActiveConversions.Remove(proto.ID);

        var ev = new RevertedEvent(target, performer, proto);
        RaiseLocalEvent(target, (object)ev, true);
        Dirty(target, component);
    }
    public bool TryConvert(EntityUid target, ProtoId<ConversionPrototype> prototype, EntityUid? performer = null, ConversionableComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        Logger.Error("1");

        if (!_prototype.TryIndex(prototype, out var proto))
            return false;

        Logger.Error("2");

        if (!CanConvert(target, prototype, performer, component))
            return false;

        Logger.Error("3");

        Convert(target, proto, performer, component);
        return true;
    }
    public void Convert(EntityUid target, ConversionPrototype proto, EntityUid? performer = null, ConversionableComponent? component = null)
    {
        if (!Resolve(target, ref component))
            return;

        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return;

        if (proto.Briefing != null)
            _antag.SendBriefing(target, Loc.GetString(proto.Briefing.Value.Text ?? ""), proto.Briefing.Value.Color, proto.Briefing.Value.Sound);

        EntityManager.AddComponents(target, registry: proto.Components);
        _role.MindAddRoles(mindId, proto.MindComponents);

        if (proto.Channels.Count > 0)
        {
            EnsureComp<IntrinsicRadioReceiverComponent>(target);
            EnsureComp<IntrinsicRadioTransmitterComponent>(target).Channels.UnionWith(proto.Channels);
            EnsureComp<ActiveRadioComponent>(target).Channels.UnionWith(proto.Channels);
        }

        var conversion = new ConversionData()
        {
            Owner = GetNetEntity(performer),
            Prototype = proto.ID,
            StartTime = _timing.CurTime,
            EndTime = proto.Duration == null ? null : _timing.CurTime + TimeSpan.FromSeconds(proto.Duration.Value),
        };

        component.ActiveConversions.Add(proto.ID, conversion);

        var ev = new ConvertedEvent(target, performer, conversion);
        RaiseLocalEvent(target, (object)ev, true);
        Dirty(target, component);
    }
    public void MindRemoveRoles(EntityUid mindId, ComponentRegistry components, MindComponent? mind = null, bool silent = false)
    {
        if (!Resolve(mindId, ref mind))
            return;

        EntityManager.RemoveComponents(mindId, components);
        var antagonist = false;
        foreach (var compReg in components.Values)
        {
            var compType = compReg.Component.GetType();

            var comp = EntityManager.ComponentFactory.GetComponent(compType);
            if (_role.IsAntagonistRole(comp.GetType()))
            {
                antagonist = true;
                break;
            }
        }

        var message = new RoleRemovedEvent(mindId, mind, antagonist);

        if (mind.OwnedEntity != null)
        {
            RaiseLocalEvent(mind.OwnedEntity.Value, message, true);
        }

        _adminLogger.Add(LogType.Mind, LogImpact.Low,
            $"Role components {string.Join(components.Keys.ToString(), ", ")} removed to mind of {_mind.MindOwnerLoggingString(mind)}");
    }
}
