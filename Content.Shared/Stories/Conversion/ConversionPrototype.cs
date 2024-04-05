using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Content.Shared.StatusIcon;
using Content.Shared.Tag;
using Robust.Shared.Serialization;
using Content.Shared.Roles;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Content.Shared.Whitelist;
using Content.Shared.Mind;

namespace Content.Shared.SpaceStories.Conversion;

[Prototype("conversion")]
[DataDefinition]
public sealed partial class ConversionPrototype : IPrototype
{
    [ViewVariables][IdDataField] public string ID { get; private set; } = default!;
    #region Convert
    [DataField("welcomeMessage")]
    public string? WelcomeMessage = null;

    [DataField("soundOnConvert")]
    public SoundSpecifier? SoundOnConvert = null;

    [DataField("componentsOnConvert", serverOnly: true)]
    [AlwaysPushInheritance]
    public EditComponentsSpecial? ComponentsOnConvert = null;

    [DataField("rolesOnConvert")]
    [AlwaysPushInheritance]
    public EditRolesSpecial? RolesOnConvert = null;
    #endregion

    #region Revert
    [DataField("goodbyeMessage")]
    public string? GoodbyeMessage = null;

    [DataField("componentsOnRevert", serverOnly: true)]
    [AlwaysPushInheritance]
    public EditComponentsSpecial? ComponentsOnRevert = null;

    [DataField("rolesOnRevert")]
    [AlwaysPushInheritance]
    public EditRolesSpecial? RolesOnRevert = null;

    [DataField("soundOnRevert")]
    public SoundSpecifier? SoundOnRevert = null;
    #endregion

    #region Special
    [DataField("channels")]
    public HashSet<string> Channels = new();

    [DataField("needMind")]
    public bool NeedMind = true;

    [DataField("revertOnMindShield")]
    public bool RevertOnMindShield = true;
    #endregion

    // #region StatusIcon
    // [DataField("statusIconWhitelist")]
    // public EntityWhitelist StatusIconWhitelist = new();

    // [DataField("statusIcon")]
    // public ProtoId<StatusIconPrototype>? StatusIcon { get; set; } = null;

    // [DataField("iconVisibleToGhost")]
    // public bool IconVisibleToGhost = true;
    // #endregion
}

[DataDefinition]
[ImplicitDataDefinitionForInheritors]
public sealed partial class EditComponentsSpecial
{
    [DataField("toAdd")]
    [AlwaysPushInheritance]
    public ComponentRegistry ToAdd = new();

    [DataField("toRemove")]
    [AlwaysPushInheritance]
    public HashSet<string> ToRemove = new();
    public void ApplyTo(EntityUid uid)
    {
        var factory = IoCManager.Resolve<IComponentFactory>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var serializationManager = IoCManager.Resolve<ISerializationManager>();

        foreach (var name in ToRemove)
        {
            var comp = factory.GetComponent(name);
            entityManager.RemoveComponent(uid, comp!.GetType());
        }

        foreach (var (name, data) in ToAdd)
        {
            var component = (Component) factory.GetComponent(name);
            var temp = (object) component;
            serializationManager.CopyTo(data.Component, ref temp);
            entityManager.RemoveComponent(uid, temp!.GetType());
            entityManager.AddComponent(uid, (Component) temp);
        }
    }
}

[DataDefinition]
[ImplicitDataDefinitionForInheritors]
public sealed partial class EditRolesSpecial
{
    [DataField("toAdd")]
    [AlwaysPushInheritance]
    public ComponentRegistry ToAdd = new();

    [DataField("toRemove")]
    [AlwaysPushInheritance]
    public HashSet<string> ToRemove = new();
    public void ApplyTo(EntityUid mindId, MindComponent mind)
    {
        var factory = IoCManager.Resolve<IComponentFactory>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var serializationManager = IoCManager.Resolve<ISerializationManager>();

        foreach (var name in ToRemove)
        {
            var comp = factory.GetComponent(name);
            entityManager.RemoveComponent(mindId, comp);
            var message = new RoleRemovedEvent(mindId, mind, true);

            if (mind.OwnedEntity != null)
            {
                entityManager.EventBus.RaiseLocalEvent(mind.OwnedEntity.Value, message, true);
            }
        }

        foreach (var (name, data) in ToAdd)
        {
            var component = (Component) factory.GetComponent(name);
            var temp = (object) component;
            serializationManager.CopyTo(data.Component, ref temp);
            entityManager.RemoveComponent(mindId, temp!.GetType());
            entityManager.AddComponent(mindId, (Component) temp);


            var mindEv = new MindRoleAddedEvent(false);
            entityManager.EventBus.RaiseLocalEvent(mindId, ref mindEv);

            var message = new RoleAddedEvent(mindId, mind, true, false);
            if (mind.OwnedEntity != null)
            {
                entityManager.EventBus.RaiseLocalEvent(mind.OwnedEntity.Value, message, true);
            }
        }
    }
}
