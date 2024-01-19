using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Vehicles
{
    public class TruckSchema : IPartsContainerSchema
    {
        public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
        {
            existingContainer ??= PartsContainerFactory.Create();
            PartsContainerSetup(existingContainer, out IPartsContainer newContainer);
            return newContainer;
        }
        public void PartsContainerSetup(IPartsContainer existingContainer, out IPartsContainer newContainer)
        {
            newContainer = existingContainer;
            EnsureSlotsHaveCorrectParts(newContainer);

            IReadOnlyList<Slot> slots = newContainer.Slots;
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetTypeRestriction(slots[0], new[] { typeof(StandardTruckBedItem), typeof(BigTruckBedItem) });
            slotRestrictionManager.SetOptional(slots[0], true);

            newContainer.SlotRestrictionManager = slotRestrictionManager;
        }

        private static void EnsureSlotsHaveCorrectParts(IPartsContainer partsContainer)
        {
            IReadOnlyList<Slot> slots = partsContainer.Slots;
            for (int i = 0; i < 1 - slots.Count; i++)
            {
                partsContainer.AddPart(new Slot(), null);
            }
            slots = partsContainer.Slots;
            IPart preexistingPart0 = slots[0].Inventory.Stacks?.FirstOrDefault()?.Item as IPart;
            if (preexistingPart0 is not IHasCustomStorageSize)
            {
                StandardTruckBedItem newBed = new StandardTruckBedItem();
                slots[0].Inventory.Stacks.First().Item = newBed;
            }
            slots[0].Name = "Truck Bed";
        }
    }
}
