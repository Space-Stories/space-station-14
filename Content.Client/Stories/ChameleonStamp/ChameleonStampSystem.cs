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

        private static readonly SlotFlags[] IgnoredSlots =
        {
            SlotFlags.PREVENTEQUIP,
            SlotFlags.NONE
        };
        private static readonly SlotFlags[] Slots = Enum.GetValues<SlotFlags>().Except(IgnoredSlots).ToArray();

        private readonly Dictionary<SlotFlags, List<string>> _data = new();

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
            Logger.Info($"Обновление спрайта для сущности с UID: {uid}, используя прототип: {proto.ID}");

            if (TryComp(uid, out SpriteComponent? sprite)
                && proto.TryGetComponent(out SpriteComponent? otherSprite, _factory))
            {
                Logger.Info($"Копирование спрайта из другого спрайт-компонента для UID: {uid}");
                sprite.CopyFrom(otherSprite);
            }
        }

        public IEnumerable<string> GetValidTargets(SlotFlags slot)
        {
            var set = new HashSet<string>();
            Logger.Info($"Получение допустимых целей для слота: {slot}");

            foreach (var availableSlot in _data.Keys)
            {
                if (slot.HasFlag(availableSlot))
                {
                    Logger.Info($"Добавление целей для слота: {availableSlot}");
                    set.UnionWith(_data[availableSlot]);
                }
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
                foreach (var slot in Slots)
                {
                    if (!_data.ContainsKey(slot))
                    {
                        _data.Add(slot, new List<string>());
                        Logger.Info($"Создан новый слот: {slot}");
                    }
                    _data[slot].Add(proto.ID);
                }
            }
        }
    }
}
