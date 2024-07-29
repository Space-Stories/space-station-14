using Robust.Shared.Utility;
using Content.Shared.Popups;
using Content.Shared.FixedPoint;
using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    // TODO: Лучше интегрировать SkillPrototype.
    public void Add(EntityUid uid, FixedPoint2 amount, ProtoId<SkillPrototype> skill, bool popup = true)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        var comp = EnsureComp<SkillsComponent>(uid);
        var proto = _prototype.Index(skill);

        if (comp.Skills.TryGetValue(skill, out var cur) && cur >= MaxExp)
            return;

        if (comp.AnySkills)
            return;

        // Добавляем модификатор. Предполагаем, что словарь отсортирован по возрастанию.
        for (var i = proto.Modifiers.Count - 1; i >= 0; i--)
        {
            var key = proto.Modifiers.ElementAt(i).Key;
            if (cur >= key)
            {
                amount *= proto.Modifiers.ElementAt(i).Value;
                break;
            }
        }

        if (amount == 0)
            return;

        if (!comp.Skills.TryAdd(skill, amount))
            if (comp.Skills[skill] + amount < MaxExp)
                comp.Skills[skill] += amount;
            else comp.Skills[skill] = MaxExp;

        Dirty(uid, comp);

        if (!popup)
            return;

        if (comp.Skills.TryGetValue(skill, out var current) && current >= MaxExp)
            _popup.PopupClient(Loc.GetString("skill-exp-full", ("skill", Loc.GetString($"skill-{skill}"))), uid, uid, PopupType.Small);
        else _popup.PopupClient(Loc.GetString("skill-exp-added", ("skill", Loc.GetString($"skill-{skill}")), ("amount", amount)), uid, uid, PopupType.Small);
    }
    public void Set(EntityUid uid, FixedPoint2 amount, string skill, bool popup = true)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        var comp = EnsureComp<SkillsComponent>(uid);

        // У него и так любые навыки.
        if (comp.AnySkills)
            return;

        if (!comp.Skills.TryAdd(skill, amount))
            comp.Skills[skill] = amount;

        Dirty(uid, comp);

        if (!popup)
            return;

        if (comp.Skills.TryGetValue(skill, out var current) && current >= MaxExp)
            _popup.PopupClient(Loc.GetString("skill-exp-full", ("skill", Loc.GetString($"skill-{skill}"))), uid, uid, PopupType.Small);
        else _popup.PopupClient(Loc.GetString("skill-exp-setted", ("skill", Loc.GetString($"skill-{skill}")), ("amount", amount)), uid, uid, PopupType.Small);
    }
    public void Reduce(EntityUid uid, FixedPoint2 amount, string skill, bool popup = true)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        var comp = EnsureComp<SkillsComponent>(uid);

        // У него и так любые навыки.
        if (comp.AnySkills)
            return;

        if (comp.Skills.TryGetValue(skill, out var current))
            comp.Skills[skill] = current - amount > MinExp ? current - amount : MinExp;

        Dirty(uid, comp);

        if (!popup)
            return;

        _popup.PopupClient(Loc.GetString("skill-exp-reduced", ("skill", Loc.GetString($"skill-{skill}")), ("amount", amount)), uid, uid, PopupType.Small);
    }
    public FixedPoint2 EnsureSkill(EntityUid uid, string skill)
    {
        var comp = EnsureComp<SkillsComponent>(uid);

        // Любые навыки на полную
        if (comp.AnySkills)
            return MaxExp;

        if (comp.Skills.TryAdd(skill, MinExp))
            return MinExp;
        else return comp.Skills[skill];
    }
}
