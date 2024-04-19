namespace Content.Server.Stories.Stasis
{
    [RegisterComponent, Access(typeof(StasisOnCollideSystem))]
    public sealed partial class StasisOnCollideComponent : Component
    {
        // seconds
        [DataField("stasisTime")]
        public int StasisTime = 60;

        [DataField("fixture")]
        public string FixtureID = "projectile";
    }
}
