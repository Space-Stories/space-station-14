namespace Content.Shared.Roles
{
    /// <summary>
    ///     Provides special hooks for when jobs get spawned in/equipped.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public abstract partial class JobSpecial
    {
        public virtual void AfterEquip(EntityUid mob) // SPACE STORIES - start
        {
            // in childs
        }
        public virtual void BeforeEquip(EntityUid mob)
        {
            // in childs
        } // SPACE STORIES - end
    }
}
