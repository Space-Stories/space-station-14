using Content.Shared.Mobs;
using Robust.Shared.Random;

namespace Content.Shared.Stories.Skills;

public abstract partial class SharedSkillsSystem
{
    private void InitializeLoss()
    {
        SubscribeLocalEvent<SkillsComponent, MobStateChangedEvent>(OnMobState);
    }
    private void OnMobState(EntityUid uid, SkillsComponent component, MobStateChangedEvent args)
    {

        // _random.NextFloat() является хардкодом,
        // так как не учитывает MinExp и MaxExp,
        // а выдает значение меж 0.0f и 1.0f.
        // Не знаю как сделать рамки для этого.

        if (args.NewMobState == MobState.Dead && _random.Prob(ExperienceLossProb))
            SetExpInSkill(uid, _random.NextFloat(), _random.Pick(_experienceLossSkills));
    }
}
