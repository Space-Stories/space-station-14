using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class SkillsAffectShotsComponent : Component
{

    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills = new()
    {
        {"Guns", 1.0f},
    };

    // GunRefreshModifiersEvent не дает данных о стреляющем,
    // поэтому мы сами сохраняем его. Может криво работать.

    public EntityUid? User;

    // Все значения ниже добавляются к текущем статам оружия, но перед этим
    // они будут умножены на силу дебаффов, которая зависит от опыта скиллов.
    // Чем ближе опыт к максимальному, тем меньше сила дебаффа.

    [ViewVariables(VVAccess.ReadWrite), DataField("minAngle")]
    public Angle MinAngle = Angle.FromDegrees(0);

    [ViewVariables(VVAccess.ReadWrite), DataField("maxAngle")]
    public Angle MaxAngle = Angle.FromDegrees(0);

    [DataField]
    public Angle AngleDecay = Angle.FromDegrees(0);

    [DataField]
    public Angle AngleIncrease = Angle.FromDegrees(0);

    [DataField]
    public float CameraRecoilScalar = 0.0f;

    [DataField]
    public float FireRate = 0.0f;

    [DataField]
    public float ProjectileSpeed = 0.0f;

    [DataField]
    public int ShotsPerBurst = 0;
}
