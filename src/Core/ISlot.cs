using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    public interface ISlot
    {
        IPart Part { get; }
        ISlotDefinition GenericDefinition { get; }
    }
    public interface ISlot<T> : ISlot where T : ISlotDefinition
    {
        T Definition { get; }
    }
    public interface ISlotDefinition
    {
        string Name { get; }
        ISlot CreateSlotInstance(WorldObject worldObject, IPartsContainer partsContainer);
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
        
    }
    public class RegularInventorySlotDefinition : ISlotDefinition, IOptionalSlotDefinition, ILimitedTypesSlotDefinition
    {
        public IEnumerable<Type> AllowedItemTypes { get; init; }
        public string Name { get; init; }
        public bool Optional { get; init; }

        public ISlot CreateSlotInstance(WorldObject worldObject, IPartsContainer partsContainer)
        {
            return new Slot();
        }
    }
    public abstract class InventorySlot : Slot
    {
        public abstract ISlotDefinition GenericDefinition { get; }

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
