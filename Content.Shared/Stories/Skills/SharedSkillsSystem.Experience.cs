using Robust.Shared.Utility;
using Content.Shared.Popups;
using Content.Shared.FixedPoint;
using System.Linq;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    public void AddExpToSkill(EntityUid uid, FixedPoint2 amount, string skill, bool popup = true)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        var comp = EnsureComp<SkillsComponent>(uid);

        if (comp.Skills.TryGetValue(skill, out var cur) && cur >= MaxExp)
            return;

        if (comp.AnySkills)
            return;

        // // Получение модификатора для получаемого опыта.
        // // К примеру, если опыт > 0.6, то модификатор 0.1.
        // // Наверное, это надо и Reduce методу.

        // // Ближайший модификатор к текущему значению.
        // var nearest = comp.SkillsModifiers.Keys.AsEnumerable().OrderBy(x => Math.Abs((long) x - (long) cur)).First();
        // // Сам модификатор.
        // var modifier = comp.SkillsModifiers[nearest];
        // // Ключ, который заключен в nearest служит так же минимальным значением опыта для операции.
        // if (cur >= nearest)
        //     amount *= modifier;


        // Добавляем модификатор. Предполагаем, что словарь отсортирован по возрастанию.
        // TODO: Сортировка словаря перед операцией.
        for (var i = comp.SkillsModifiers.Count - 1; i >= 0; i--)
        {
            var key = comp.SkillsModifiers.ElementAt(i).Key;
            if (cur >= key)
            {
                amount *= comp.SkillsModifiers.ElementAt(i).Value;
                break;
            }
        }

        if (amount == 0)
            return;

        if (!comp.Skills.TryAdd(skill, amount))
            if (comp.Skills[skill] + amount < MaxExp)
                comp.Skills[skill] += amount;
            else comp.Skills[skill] = MaxExp;

        if (!popup)
            return;

        if (comp.Skills.TryGetValue(skill, out var current) && current >= MaxExp)
            _popup.PopupEntity(Loc.GetString("skill-exp-full", ("skill", Loc.GetString($"skill-{skill}"))), uid, uid, PopupType.Small);
        else _popup.PopupEntity(Loc.GetString("skill-exp-added", ("skill", Loc.GetString($"skill-{skill}")), ("amount", amount)), uid, uid, PopupType.Small);
    }
    public void SetExpInSkill(EntityUid uid, FixedPoint2 amount, string skill, bool popup = true)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        var comp = EnsureComp<SkillsComponent>(uid);

        // У него и так любые навыки.
        if (comp.AnySkills)
            return;

        if (!comp.Skills.TryAdd(skill, amount))
            comp.Skills[skill] = amount;

        if (!popup)
            return;

        if (comp.Skills.TryGetValue(skill, out var current) && current >= MaxExp)
            _popup.PopupEntity(Loc.GetString("skill-exp-full", ("skill", Loc.GetString($"skill-{skill}"))), uid, uid, PopupType.Small);
        else _popup.PopupEntity(Loc.GetString("skill-exp-setted", ("skill", Loc.GetString($"skill-{skill}")), ("amount", amount)), uid, uid, PopupType.Small);
    }
    public void ReduceExpFromSkill(EntityUid uid, FixedPoint2 amount, string skill, bool popup = true)
    {
        DebugTools.Assert(amount >= MinExp && amount <= MaxExp);

        var comp = EnsureComp<SkillsComponent>(uid);

        // У него и так любые навыки.
        if (comp.AnySkills)
            return;

        if (comp.Skills.TryGetValue(skill, out var current))
            comp.Skills[skill] = current - amount > MinExp ? current - amount : MinExp;

        if (!popup)
            return;

        _popup.PopupEntity(Loc.GetString("skill-exp-reduced", ("skill", Loc.GetString($"skill-{skill}")), ("amount", amount)), uid, uid, PopupType.Small);
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
