using Eco.Core.Controller;
using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.TextLinks;
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
        public LocString DisplayRestriction(Slot slot);
        public bool IsOptional(Slot slot);
        public bool CanAddItemToSlot(Slot slot, Item item, out List<LocString> failureReasons);
        public bool CanRemoveItemFromSlot(Slot slot, Item item, out List<LocString > failureReasons);
        bool IsSlotEnabled(Slot slot);
        public ThreadSafeAction<Slot> SlotEnabledChangedEvent { get; }
    }
    public class RequireEmptyStorageRestriction : InventoryRestriction
    {
        public override LocString Message => Localizer.DoStr("Storage must be empty");
        public Inventory Inventory { get; }

        public RequireEmptyStorageRestriction(Inventory inventory)
        {
            Inventory = inventory;
        }
        public override int MaxAccepted(RestrictionCheckData checkData, Item item, int currentQuantity)
        {
            if (!Inventory.IsEmpty) return 0;
            return base.MaxAccepted(checkData, item, currentQuantity);
        }
        public override int MaxPickup(RestrictionCheckData checkData, Item item, int currentQuantity)
        {
            if (!Inventory.IsEmpty) return 0;
            return base.MaxPickup(checkData, item, currentQuantity);
        }
    }
    public class NoRemoveRestriction : InventoryRestriction
    {
        public override int Priority => 1000;
        public override LocString Message => Localizer.DoStr("Cannot remove from this slot");
        public override int MaxPickup(RestrictionCheckData checkData, Item item, int currentQuantity) => 0;
    }
    public class BasicSlotRestrictionManager : ISlotRestrictionManager
    {
        public IPartsContainer PartsContainer { get; private set; }
        private IDictionary<Slot, ISet<Type>> ValidItemTypesBySlot { get; } = new ThreadSafeDictionary<Slot, ISet<Type>>();
        private IDictionary<Slot, SpecificItemTypesRestriction> SlotTypeInventoryRestrictions { get; } = new ThreadSafeDictionary<Slot, SpecificItemTypesRestriction>();
        public ISet<Slot> DisabledSlots { get; } = new HashSet<Slot>();

        public ThreadSafeAction<Slot> SlotEnabledChangedEvent { get; } = new ThreadSafeAction<Slot>();

        public void AddRestriction(Slot slot, InventoryRestriction inventoryRestriction)
        {
            slot.Inventory.AddInvRestriction(inventoryRestriction);
        }
        public bool IsSlotEnabled(Slot slot) => !DisabledSlots.Contains(slot);
        public void SetSlotEnabled(Slot slot, bool enabled)
        {
            Log.WriteLine(Localizer.DoStr("Slot:" + slot.Name + " was " + (enabled ? "enabled" : "disabled")));
            if (enabled)
            {
                if (DisabledSlots.Remove(slot))
                {
                    SlotEnabledChangedEvent.Invoke(slot);
                }
            }
            else if (DisabledSlots.Add(slot))
            {
                SlotEnabledChangedEvent.Invoke(slot);
            }
        }
        public void SetOptional(Slot slot, bool isOptional)
        {
            if (isOptional) slot.Inventory.RemoveAllRestrictions(restriction => restriction is NoRemoveRestriction);
            else slot.Inventory.AddInvRestriction(new NoRemoveRestriction());
        }
        public void SetTypeRestriction(Slot slot, IEnumerable<Type> validItemTypes)
        {
            lock (ValidItemTypesBySlot)
            {
                if (ValidItemTypesBySlot.TryGetValue(slot, out ISet<Type> currentValidItemTypes))
                {
                    currentValidItemTypes.Clear();
                    currentValidItemTypes.AddRange(validItemTypes);
                    RemoveExistingInventoryRestriction(slot);
                }
                else
                {
                    currentValidItemTypes = new HashSet<Type>(validItemTypes);
                    ValidItemTypesBySlot.Add(slot, currentValidItemTypes);
                }
                AddTypeInventoryRestriction(slot, currentValidItemTypes);
            }
        }
        private void AddTypeInventoryRestriction(Slot slot, ISet<Type> currentValidItemTypes)
        {
            SpecificItemTypesRestriction typeRestriction = new SpecificItemTypesRestriction(currentValidItemTypes.ToArray());
            slot.Inventory.AddInvRestriction(typeRestriction);
            SlotTypeInventoryRestrictions[slot] = typeRestriction;
        }

        private void RemoveExistingInventoryRestriction(Slot slot)
        {
            InventoryRestriction inventoryRestriction = SlotTypeInventoryRestrictions[slot];
            IEnumerable<InventoryRestriction> otherRestrictions = slot.Inventory.Restrictions.Except(new[] { inventoryRestriction });
            slot.Inventory.ClearRestrictions();
        }

        private List<Item> AllowedItems(Slot slot)
        {
            List<Item> allowedItems = new List<Item>();
            if (ValidItemTypesBySlot.TryGetValue(slot, out ISet<Type> validItemTypes))
            {
                foreach(Type type in validItemTypes)
                {
                    Item item = Item.Get(type);
                    if (item != null) allowedItems.Add(item);
                }
            }
            return allowedItems;
        }
        public bool CanAddItemToSlot(Slot slot, Item item, out List<LocString> failureReasons)
        {
            if (!ValidItemTypesBySlot.TryGetValue(slot, out ISet<Type> validItemTypes))
            {
                failureReasons = new List<LocString>() { Localizer.DoStr("This slot does not accept items") };
                return false;
            }
            if (!validItemTypes.Contains(item.Type))
            {
                failureReasons = new List<LocString>();
                IEnumerable<LocString> allowedItems = AllowedItems(slot).Select(allowedItem => allowedItem.UILink());

                LocString failureReason = Localizer.DoStr("Slot only accepts").AppendSpaceIfSet() + allowedItems.NewlineList();
                return false;
            }
            failureReasons = new List<LocString>();
            return true;
        }

        public bool CanRemoveItemFromSlot(Slot slot, Item item, out List<LocString> failureReasons)
        {
            failureReasons = new List<LocString>();
            return true;
        }

        public LocString DisplayRestriction(Slot slot)
        {
            List<Item> allowedItems = AllowedItems(slot);
            return Localizer.DoStr("Accepts").AppendSpaceIfSet() + allowedItems.Select(item => item.UILink()).CommaList();
        }

        public bool IsOptional(Slot slot)
        {
            return !slot.Inventory.Restrictions.Any(restriction => restriction is NoRemoveRestriction);
        }
    }
}
