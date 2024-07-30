using Content.Shared.FixedPoint;

namespace Content.Shared.Stories.Skills;

public interface IReceivesSkills
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; protected set; }
}
