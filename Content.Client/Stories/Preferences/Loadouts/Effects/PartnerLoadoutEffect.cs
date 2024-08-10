using System.Diagnostics.CodeAnalysis;
using Content.Client.Corvax.Sponsors;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

public sealed partial class PartnerLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public float MinTier = 1f;

    public override bool Validate(HumanoidCharacterProfile profile, RoleLoadout loadout, ICommonSession? session, IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        var manager = collection.Resolve<SponsorsManager>();
        reason = FormattedMessage.FromUnformatted(Loc.GetString("loadout-group-partner-tier-restriction"));
        if (!manager.TryGetInfo(out var sponsorInfo) || sponsorInfo.Tier < MinTier)
        {
            return false;
        }

        return true;
    }
}
