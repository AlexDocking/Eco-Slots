using Eco.Core.Utils;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    public interface ISlotRestrictionManager
    {
        bool CanAcceptPart(IPart part, out List<LocString> failureReasons);
        bool CanRemovePart(IPart part, out List<LocString> failureReasons);
        bool CanSetPart(IPart part, out List<LocString> failureReasons);
    }
    /// <summary>
    /// Used by <see cref="InventorySlot"/> to check whether the slot can accept an incoming part and if the existing part can be removed or not.
    /// TODO: the design for slot restrictions needs more work.
    /// </summary>
    public sealed class InventorySlotRestrictionManager : ISlotRestrictionManager
    {
        public InventorySlotRestrictionManager(InventorySlot slot)
        {
            Slot = slot;
        }
        public InventorySlot Slot { get; }
        private Inventory Inventory => Slot.Inventory;

        /// <summary>
        /// Add the necessary <see cref="InventoryRestriction"/>s to the public storage.
        /// As it stands it can only handle pre-defined restriction types.
        /// </summary>
        public void CreateInventoryRestrictionsBasedOnWorldObject(WorldObject worldObject)
        {
            if (worldObject == null) return;
            worldObject.TryGetComponent(out PublicStorageComponent publicStorage);
            Inventory storage = publicStorage?.Storage;
            if (!Slot.SlotDefinition.CanPartEverBeRemoved)
            {
                NoRemoveRestriction restriction = new NoRemoveRestriction();
                Inventory.AddInvRestriction(restriction);
            }
            if (Slot.SlotDefinition.CanPartEverBeAdded)
            {
                if (Slot.SlotDefinition.RestrictionsToAddPart.FirstOrDefault(restrictionToAdd => restrictionToAdd is LimitedTypeSlotRestriction) is LimitedTypeSlotRestriction limitedTypeSlotRestriction)
                {
                    EditableSpecificItemTypesRestriction restriction = new EditableSpecificItemTypesRestriction();
                    restriction.AllowedItemTypes.AddRange(limitedTypeSlotRestriction.AllowedTypes);
                    Inventory.AddInvRestriction(restriction);
                }
            }
            if (Slot.SlotDefinition.RestrictionsToAddPart.Any(restrictionToAdd => restrictionToAdd is RequiresEmptyPublicStorageToAddSlotRestriction))
            {
                RequireEmptyStorageToAddRestriction restriction = new RequireEmptyStorageToAddRestriction();
                if (storage != null)
                {
                    restriction.InventorySet.Inventories.Add(storage);
                    Inventory.AddInvRestriction(restriction);
                }
            }
            if (Slot.SlotDefinition.RestrictionsToRemovePart.Any(restrictionToRemove => restrictionToRemove is RequiresEmptyPublicStorageToRemoveSlotRestriction))
            {
                RequireEmptyStorageToRemoveRestriction restriction = new RequireEmptyStorageToRemoveRestriction();
                if (storage != null)
                {
                    restriction.InventorySet.Inventories.Add(storage);
                    Inventory.AddInvRestriction(restriction);
                }
            }
        }
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
            if (Inventory.GetMaxAcceptedVal(partItem, Inventory.Stacks.First().Quantity) > 0)
            {
                return true;
            }
            return false;
        }
        public bool CanRemovePart(IPart part, out List<LocString> failureReasons)
        {
            failureReasons = new List<LocString>();
            if (Inventory.IsEmpty)
            {
                failureReasons.Add(Localizer.DoStr("Inventory is empty"));
                return false;
            }

            if (part is not Item partItem)
            {
                failureReasons.Add(Localizer.Do($"{part?.GetType().Name ?? "null"} is not an Item"));
                return false;
            }
            
            IEnumerable<InventoryRestriction> violatedRestrictions = Inventory.Restrictions.Where(restriction => restriction.MaxPickup(RestrictionCheckData.New(Inventory, null, null), partItem, 1) == 0);

            if (violatedRestrictions.Any())
            {
                //Inventory.TryGetBestRestrictionMessage(violatedRestrictions, out LocString failureMessage);
                failureReasons.Add(Localizer.DoStr("Locked until storage is empty"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Whether the slot is currently preventing parts of any type from being inserted.
        /// </summary>
        /// <returns></returns>
        public Result CanAcceptAnyPart()
        {
            InventoryRestriction violatedRequireEmptyStorageRestriction = Inventory.Restrictions.FirstOrDefault(restriction => restriction is RequireEmptyStorageToAddRestriction requireEmptyStorageToAddRestriction && requireEmptyStorageToAddRestriction.MaxAccepted(null, 0) == 0);
            if (violatedRequireEmptyStorageRestriction == null) return Result.Succeeded;
            return Result.Fail(Localizer.DoStr("Locked until storage is empty"));
        }
    }
}
