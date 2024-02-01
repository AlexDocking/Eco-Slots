using Eco.Core.Utils;
using Eco.Gameplay.Items;
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
            if (Inventory.GetMaxAcceptedVal(partItem, Inventory.Stacks.First().Quantity) > 0)
            {
                return true;
            }
            return false;
        }
        public bool CanRemovePart(IPart part, out List<LocString> failureReasons)
        {
            failureReasons = new List<LocString>();
            if (Inventory.IsEmpty) return false;

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

        public Result CanAcceptAnyPart()
        {
            var violatedRequireEmptyStorageRestriction = Inventory.Restrictions.FirstOrDefault(restriction => restriction is RequireEmptyStorageToAddRestriction requireEmptyStorageToAddRestriction && requireEmptyStorageToAddRestriction.MaxAccepted(null, 0) == 0);
            if (violatedRequireEmptyStorageRestriction == null) return Result.Succeeded;
            return Result.Fail(Localizer.DoStr("Locked until storage is empty"));
        }
    }
}
