using Eco.Gameplay.Items;
using Eco.Shared.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts.WIP
{
    /*public class PartsContainerFactory
    {
        public IPartsContainer Create(IPartsContainerDefinition definition)
        {
            IPartsContainer partsContainer;
            foreach(ISlotDefinition slotDefinition in definition.SlotDefinitions)
            {
                ISlot slot = CreateSlot(slotDefinition);
                partsContainer.AddSlot(slot);
            }
            return partsContainer;
        }
        public ISlot CreateSlot(ISlotDefinition definition) { }
    }
    public class TooltipLibrary
    {
        Dictionary<Type, ISlotTooltip> SlotTooltipServices { get; }
        public LocString ItemPartsContainerTooltip(IHasPartsContainerDefinition itemWithParts)
        {
            LocString tooltip;
            foreach(var slotDefinition in itemWithParts.Definition.SlotDefinitions)
            {
                tooltip += ItemSlotDefinitionTooltip(slotDefinition);
            }
        }
        public LocString ItemSlotDefinitionTooltip(ISlotDefinition slotDefinition)
        {
            LocString tooltip;
            tooltip += slotDefinition.SlotName;
            tooltip += 
        }
    }
    public interface ISlotTooltip
    {
        bool CanCreateTooltip(ISlot slot, out int priority);
        LocString DescribeSlot(ISlot slot);
    }
    public class InventorySlotTooltip : ISlotTooltip
    {
        public bool CanCreateTooltip(ISlot slot, out int priority)
        {
            priority = 0;
            return slot is InventorySlot;
        }

        public LocString DescribeSlot(ISlot slot);
    }
    public interface IHasPartsContainerDefinition
    {
        IPartsContainerDefinition Definition { get; }
    }
    public class TruckItem : Item, IHasPartsContainerDefinition
    {
        public IPartsContainerDefinition Definition { get; }
    }
    public interface IPartsContainerDefinition
    {
        IReadOnlyList<ISlotDefinition> SlotDefinitions { get; }
    }
    public interface ISlot
    {
        IPart Part { get; }
        ISlotDefinition Definition { get; }
    }
    public class InventorySlot : ISlot
    {
        
    }
    public interface ISlotDefinition
    {
        string SlotName { get; }
        LocString DescribeRestrictions();
    }
    public interface IPartsContainer
    {
        IReadOnlyList<ISlot> Slots { get; }
        IPartsContainerDefinition Definition { get; }
    }
    */
}
