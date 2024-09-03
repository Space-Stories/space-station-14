using System.Linq;
using Content.Shared.Stories.ChameleonStamp;
using Content.Shared.Inventory;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Log;
using Content.Shared.Paper;

namespace Content.Client.Stories.ChameleonStamp
{
    public sealed class ChameleonStampSystem : SharedChameleonStampSystem
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly IComponentFactory _factory = default!;

        private readonly List<string> _data = new List<string>();

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ChameleonStampComponent, AfterAutoHandleStateEvent>(HandleState);
            PrepareAllVariants();
            SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReloaded);
        }

        private void OnProtoReloaded(PrototypesReloadedEventArgs args)
        {
            if (args.WasModified<EntityPrototype>())
            {
                PrepareAllVariants();
            }
        }

        private void HandleState(EntityUid uid, ChameleonStampComponent component, ref AfterAutoHandleStateEvent args)
        {
            Logger.Info($"Обработка состояния для сущности с UID: {uid}");
            UpdateVisuals(uid, component);
        }

        protected override void UpdateSprite(EntityUid uid, EntityPrototype proto)
        {
            base.UpdateSprite(uid, proto);

            if (TryComp(uid, out SpriteComponent? sprite)
                && proto.TryGetComponent(out SpriteComponent? otherSprite, _factory))
            {
                sprite.CopyFrom(otherSprite);
            }
        }

        public IEnumerable<string> GetValidTargets()
        {
            var set = new HashSet<string>();

            foreach (var proto in _data)
            {
                Logger.Info($"Добавление прототипа {proto} в список");
                set.UnionWith(_data);
            }
            return set;
        }

        private void PrepareAllVariants()
        {
            _data.Clear();
            var prototypes = _proto.EnumeratePrototypes<EntityPrototype>();

            foreach (var proto in prototypes)
            {
                // проверка, является ли это допустимой одеждой
                if (!IsValidTarget(proto))
                {
                    continue;
                }
                if (!proto.TryGetComponent(out StampComponent? item, _factory))
                {
                    continue;
                }
                _data.Add(proto.ID);
                Logger.Info($"Добавлен прототип {proto.ID}");
            }
        }
    }
}
