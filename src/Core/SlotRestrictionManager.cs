using Eco.Core.Controller;
using Eco.Core.Items;
using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    public interface ISlotRestrictionManager
    {
        bool CanAcceptPart(IPart part, out List<LocString> failureReasons);
        bool CanRemovePart(IPart part, out List<LocString> failureReasons);
        bool CanSetPart(IPart part, out List<LocString> failureReasons);
    }
    public class InventorySlotRestrictionManager : ISlotRestrictionManager
    {
        public InventorySlotRestrictionManager(ISlot slot, Inventory inventory)
        {
            Slot = slot;
            Inventory = inventory;
        }
        public ISlot Slot { get; }
        public Inventory Inventory { get; }

        public bool CanSetPart(IPart part, out List<LocString> failureReasons)
        {
            if (!CanAcceptPart(part, out failureReasons)) return false;
            return Slot.Part == null || CanRemovePart(part, out failureReasons);
        }
        public bool CanAcceptPart(IPart part, out List<LocString> failureReasons)
        {
            failureReasons = new List<LocString>();
            if (part is not Item partItem)
            {
                failureReasons.Add(Localizer.Do($"{part?.GetType().Name ?? "null"} is not an Item"));
                return false;
            }
            if (Inventory.AcceptsItem(partItem)) return true;
            return false;
        }
        public bool CanRemovePart(IPart part, out List<LocString> failureReasons)
        {
            failureReasons = new List<LocString>();
            if (part is not Item partItem)
            {
                failureReasons.Add(Localizer.Do($"{part?.GetType().Name ?? "null"} is not an Item"));
                return false;
            }
            
            IEnumerable<InventoryRestriction> violatedRestrictions = Inventory.Restrictions.Where(restriction => restriction.MaxPickup(RestrictionCheckData.New(Inventory, null, null), partItem, 1) == 0);
            if (violatedRestrictions.Any())
            {
                Inventory.TryGetBestRestrictionMessage(violatedRestrictions, out LocString failureMessage);
                failureReasons.Add(failureMessage);
                return false;
            }
            return true;
        }
    }
    public interface IPartsContainerSlotRestrictionManager
    {
        public LocString DisplayRestriction(ISlot slot);
        public bool IsOptional(ISlot slot);
        bool IsSlotLocked(ISlot slot);
        IEnumerable<Type> AllowedItemTypes(ISlot slot);

        public ThreadSafeAction<ISlot> SlotLockedChangedEvent { get; }
    }

    public class BasicPartsContainerSlotRestrictionManager : IPartsContainerSlotRestrictionManager
    {
        public IPartsContainer PartsContainer { get; private set; }
        public ThreadSafeAction<ISlot> SlotLockedChangedEvent { get; } = new ThreadSafeAction<ISlot>();
        private IDictionary<ISlot, PerSlotRestrictions> SlotRestrictions { get; } = new ThreadSafeDictionary<ISlot, PerSlotRestrictions>();
        private PerSlotRestrictions GetOrAddRestrictionsToSlot(ISlot slot)
        {
            InventorySlot inventorySlot = slot as InventorySlot;
            if (inventorySlot == null) throw new ArgumentException(nameof(slot), "Slot is not InventorySlot");
            PerSlotRestrictions perSlotRestrictions;
            if (!SlotRestrictions.TryGetValue(slot, out perSlotRestrictions))
            {
                perSlotRestrictions = new PerSlotRestrictions(inventorySlot);
                if (SlotRestrictions.TryAdd(slot, perSlotRestrictions))
                {
                    perSlotRestrictions.PropertyChanged += (object _, PropertyChangedEventArgs args) =>
                    {
                        if (args.PropertyName == nameof(PerSlotRestrictions.IsLocked)) SlotLockedChangedEvent.Invoke(slot);
                    };
                }
            }
            perSlotRestrictions = SlotRestrictions[slot];
            return perSlotRestrictions;
        }
        public void SetOptional(ISlot slot, bool isOptional)
        {
            PerSlotRestrictions slotRestrictions = GetOrAddRestrictionsToSlot(slot);
            slotRestrictions.IsSlotOptional = isOptional;
        }
        public void AddRequiredEmptyStorage(ISlot slot, Inventory mustBeEmpty) => GetOrAddRestrictionsToSlot(slot).AddEmptyRequirementToStorage(mustBeEmpty);
        public void SetTypeRestriction(ISlot slot, IEnumerable<Type> validItemTypes) => GetOrAddRestrictionsToSlot(slot).AllowedItemTypes = validItemTypes;
        public IEnumerable<Type> AllowedItemTypes(ISlot slot) => GetOrAddRestrictionsToSlot(slot).AllowedItemTypes;
        public LocString DisplayRestriction(ISlot slot)
        {
            IEnumerable<Item> allowedItems = AllowedItemTypes(slot).Select(type => Item.Get(type)).NonNull();
            return Localizer.DoStr("Accepts").AppendSpaceIfSet() + allowedItems.Select(item => item.UILink()).CommaList();
        }
        public bool IsSlotLocked(ISlot slot) => GetOrAddRestrictionsToSlot(slot).IsLocked;
        public bool IsOptional(ISlot slot) => GetOrAddRestrictionsToSlot(slot).IsSlotOptional;
    }
}
