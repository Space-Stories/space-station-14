using Robust.Shared.Utility;
using Content.Shared.Popups;
using Content.Shared.FixedPoint;
using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    private void ApplyModificator(EntityUid uid, string id, ref FixedPoint2 amount, SkillPrototype proto)
    {
        amount *= (1f - EnsureSkill(uid, id) / MaxExp) * proto.Modifier;
    }

    #region API
    public FixedPoint2 EnsureSkill(EntityUid uid, string id)
    {
        var comp = EnsureComp<SkillsComponent>(uid);

        if (comp.AnySkills)
            return MaxExp;

        if (comp.Skills.TryGetValue(id, out var value))
            return value;

        return MinExp;
    }

    public bool TrySet(EntityUid uid, ProtoId<SkillPrototype> protoId, FixedPoint2 amount)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        if (!_prototype.TryIndex(protoId, out var proto))
            return false;

        var comp = EnsureComp<SkillsComponent>(uid);

        if (comp.AnySkills)
            return false;

        ApplyModificator(uid, protoId, ref amount, proto);

        Set(uid, protoId, amount, comp);

        return true;
    }

    public bool TryAdd(EntityUid uid, ProtoId<SkillPrototype> protoId, FixedPoint2 amount)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        if (!_prototype.TryIndex(protoId, out var proto))
            return false;

        var comp = EnsureComp<SkillsComponent>(uid);

        if (comp.AnySkills)
            return false;

        ApplyModificator(uid, protoId, ref amount, proto);

        Add(uid, protoId, amount, comp);

        return true;
    }

    public bool TryReduce(EntityUid uid, ProtoId<SkillPrototype> protoId, FixedPoint2 amount)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        if (!_prototype.TryIndex(protoId, out var proto))
            return false;

        var comp = EnsureComp<SkillsComponent>(uid);

        if (comp.AnySkills)
            return false;

        ApplyModificator(uid, protoId, ref amount, proto);

        Reduce(uid, protoId, amount, comp);

        return true;
    }

    public void Set(EntityUid uid, string id, FixedPoint2 amount, SkillsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.Skills.TryAdd(id, amount))
            component.Skills[id] = amount;

        Dirty(uid, component);
    }

    public void Add(EntityUid uid, string id, FixedPoint2 amount, SkillsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!component.Skills.TryAdd(id, amount))
            component.Skills[id] += amount;

        Dirty(uid, component);
    }

    public void Reduce(EntityUid uid, string id, FixedPoint2 amount, SkillsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.Skills.ContainsKey(id))
            component.Skills[id] -= amount;

        Dirty(uid, component);
    }
    #endregion
}
