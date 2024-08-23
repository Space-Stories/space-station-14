namespace Content.Server.Stories.EnergyCores
{
    [RegisterComponent]
    public sealed partial class EnergyCoreKeyComponent : Component
    {
        [DataField][ViewVariables(VVAccess.ReadOnly)]
        public EnergyCoreKeyState Key = EnergyCoreKeyState.None;
    }
}
