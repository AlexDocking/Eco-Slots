using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    [Serialized]
    public interface ISlot
    {
        string Name { get; }

        IPart Part { get; }
        ISlotDefinition GenericDefinition { get; }
        IPartsContainer PartsContainer { get; }
        ThreadSafeAction NewPartInSlotEvent { get; }
        ThreadSafeAction<ISlot, IPart, IPartProperty> PartPropertyChangedEvent { get; }

        bool CanAcceptPart(IPart validPart);
        void Initialize(WorldObject worldObject, IPartsContainer partsContainer);
        bool SetPart(IPart part);
        Result TryAddPart(IPart part);
    }
    public interface ISlot<T> : ISlot where T : ISlotDefinition
    {
        T Definition { get; }
    }
    public interface ISlotDefinition
    {
        string Name { get; }
    }
    public interface IOptionalSlotDefinition : ISlotDefinition
    {
        bool Optional { get; }
    }
    public interface ILimitedTypesSlotDefinition : ISlotDefinition
    {
        IEnumerable<Type> AllowedItemTypes { get; }
    }
    public interface IRequireEmptyStorageSlotDefinition : ISlotDefinition
    {
        bool RequiresEmptyStorageToChangePart { get; }
    }
    public class InventorySlot<T> : InventorySlot where T : ISlotDefinition
    {
        public InventorySlot()
        {
            Definition = SlotDefinitionRegister.GetDefinitionInstance<T>();
        }
        public T Definition { get; }
        public override ISlotDefinition GenericDefinition => Definition;
    }
    public static class SlotDefinitionRegister
    {
        public static ISet<ISlotDefinition> SlotDefinitions { get; } = new HashSet<ISlotDefinition>();
        public static T GetDefinitionInstance<T>() where T : ISlotDefinition
        {
            return (T)SlotDefinitions.FirstOrDefault(def => def.GetType() == typeof(T));
        }
    }
}
