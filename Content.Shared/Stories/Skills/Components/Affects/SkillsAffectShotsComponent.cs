using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Stories.Skills;

[RegisterComponent]
public sealed partial class SkillsAffectShotsComponent : Component, ISkillsAffects
{
    [DataField("skills")]
    public Dictionary<string, FixedPoint2> Skills { get; set; } = new()
    {
        {"Guns", 1.0f},
    };

    /// <summary>
    /// GunRefreshModifiersEvent не дает данных о стреляющем,
    /// поэтому мы сами сохраняем его. Может криво работать.
    /// </summary>
    [DataField]
    public EntityUid? User;

    [DataField]
    public Angle MinAngle = Angle.FromDegrees(0);

    [DataField]
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
