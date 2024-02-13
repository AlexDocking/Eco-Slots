using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    /// <summary>
    /// A slot which holds a single part.
    /// A part does not have to be a single item, hence a slot does not have to have an Inventory.
    /// A slot could also store a stack of items rather than just one, so long as the 'part' is a wrapper for the whole stack.
    /// Mods are then free to create new types of parts which are not items, such as a fridge thermostat with variable temperature,
    /// which does not need to be an item.
    /// </summary>
    [Serialized]
    public interface ISlot
    {
        string Name { get; }
        IPart Part { get; }
        /// <summary>
        /// The definition this slot is based on. It has information about 
        /// </summary>
        ISlotDefinition SlotDefinition { get; }
        /// <summary>
        /// Parent container this slot belongs to.
        /// </summary>
        IPartsContainer PartsContainer { get; }
        ThreadSafeAction NewPartInSlotEvent { get; }
        /// <summary>
        /// Called whenever any of the part's properties e.g. colour changes.
        /// </summary>
        ThreadSafeAction<ISlot, IPart, IPartProperty> PartPropertyChangedEvent { get; }
        Result CanAcceptPart(IPart validPart);
        Result CanRemovePart();
        Result CanSetPart(IPart part);
        /// <summary>
        /// After the world object is created and this slot has been added to the parts container,
        /// the container will inform this slot of its parent and the world object it belongs to.
        /// </summary>
        void Initialize(WorldObject worldObject, IPartsContainer partsContainer);
        bool SetPart(IPart part);
        Result TryAddPart(IPart part);
        Result TrySetPart(IPart part);
        /// <summary>
        /// Describe the slot, including the part, or, if it is empty, what are accepted parts.
        /// </summary>
        /// <returns></returns>
        LocString Tooltip();
    }
    /// <summary>
    /// Defines how a slot should operate, and can create tooltip sections to describe the slot in generic terms on the generic WorldObjectItem tooltip
    /// so that players know before they place down the object what the slots are.
    /// TODO: possibly too tightly coupled to <see cref="InventorySlot"/>
    /// </summary>
    public interface ISlotDefinition
    {
        string Name { get; }
        bool CanPartEverBeAdded { get; }
        bool CanPartEverBeRemoved { get; }
        IEnumerable<ISlotAddRestriction> RestrictionsToAddPart { get; }
        IEnumerable<ISlotRemoveRestriction> RestrictionsToRemovePart { get; }
        /// <summary>
        /// Create a new slot based on this definitions. Probably needs refactoring.
        /// </summary>
        /// <returns></returns>
        ISlot MakeSlotFromDefinition();
        /// <summary>
        /// Return a description of the restrictions in general terms, without any instance-specific information.
        /// </summary>
        /// <returns></returns>
        LocString TooltipContent();
        /// <summary>
        /// Return a title for the slot, such as 'Slot: {Name}'
        /// </summary>
        /// <returns></returns>
        LocString TooltipTitle();
    }
}
