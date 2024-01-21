using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
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
            PartsContainerSetup(worldObject, existingContainer, out IPartsContainer newContainer);
            return newContainer;
        }
        public void PartsContainerSetup(WorldObject worldObject, IPartsContainer existingContainer, out IPartsContainer newContainer)
        {
            newContainer = existingContainer;
            EnsureSlotsHaveCorrectParts(newContainer);

            IReadOnlyList<Slot> slots = newContainer.Slots;
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetTypeRestriction(slots[0], new[] { typeof(StandardTruckBedItem), typeof(BigTruckBedItem) });
            slotRestrictionManager.SetOptional(slots[0], true);


            Inventory publicStorage = worldObject.GetComponent<PublicStorageComponent>().Storage;

            slotRestrictionManager.AddRequiredEmptyStorage(slots[0], publicStorage);

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
                slots[0].SetPart(newBed);
            }
            slots[0].Name = "Truck Bed";
        }
    }
}
