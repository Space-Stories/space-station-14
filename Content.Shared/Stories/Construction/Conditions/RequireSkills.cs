using Content.Shared.FixedPoint;
using Content.Shared.Maps;
using Content.Shared.Stories.Skills;
using Content.Shared.Tag;
using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Shared.Construction.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class RequiresSkills : IConstructionCondition, IRequiresSkills
    {
        [DataField("skills")]
        public Dictionary<string, FixedPoint2> Skills { get; set; } = new()
        {
        { "Engineering", 0.5f },
        };
        public bool Condition(EntityUid user, EntityCoordinates location, Direction direction)
        {
            var entManager = IoCManager.Resolve<IEntityManager>();
            var sysMan = entManager.EntitySysManager;
            var skillsSystem = sysMan.GetEntitySystem<SharedSkillsSystem>();

            return skillsSystem.HasSkills(user, this);
        }

        public ConstructionGuideEntry GenerateGuideEntry()
        {
            return new ConstructionGuideEntry
            {
                Localization = "construction-step-condition-require-skills"
            };
        }
    }
}
