using Content.Shared.Stories.Lib;
using Content.Shared.Stories.Lib.Invisibility;

namespace Content.Server.Stories.Lib;

/// <summary>
/// A system that combines common methods from systems made by Space Stories
/// And containing shortcuts for Space Wizards code
/// </summary>
public sealed partial class StoriesUtilsSystem : SharedStoriesUtilsSystem
{
    public void MakeInvisible(EntityUid uid)
    {
        EnsureComp<InvisibleComponent>(uid);
    }

    public void MakeVisible(EntityUid uid)
    {
        if (HasComp<InvisibleComponent>(uid))
        {
            RemCompDeferred<InvisibleComponent>(uid);
        }
    }
}
