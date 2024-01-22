﻿using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Kitchen
{
    public class KitchenBaseCabinetSchema : IPartsContainerSchema
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
            slotRestrictionManager.SetTypeRestriction(slots[0], new[] { typeof(KitchenBaseCabinetBoxItem) });
            slotRestrictionManager.SetOptional(slots[0], false);

            slotRestrictionManager.SetTypeRestriction(slots[1], new[] { typeof(KitchenCupboardWorktopItem) });
            slotRestrictionManager.SetOptional(slots[1], false);

            slotRestrictionManager.SetTypeRestriction(slots[2], new[] { typeof(KitchenCabinetFlatDoorItem), typeof(KitchenCupboardRaisedPanelDoorItem) });
            slotRestrictionManager.SetOptional(slots[2], true);

            newContainer.SlotRestrictionManager = slotRestrictionManager;
        }

        private static void EnsureSlotsHaveCorrectParts(IPartsContainer partsContainer)
        {
            IReadOnlyList<Slot> slots = partsContainer.Slots;
            for (int i = 0; i < 3 - slots.Count; i++)
            {
                partsContainer.AddPart(new Slot(), null);
            }
            IPart preexistingPart0 = slots[0].Part;
            IPart preexistingPart1 = slots[1].Part;

            if (preexistingPart0 is not KitchenBaseCabinetBoxItem)
            {
                KitchenBaseCabinetBoxItem newBox = new KitchenBaseCabinetBoxItem();
                if (slots[0].Part is IHasModelPartColour colourComponent) newBox.ColourData.Colour = colourComponent.ColourData.Colour;
                slots[0].SetPart(newBox);
            }

            if (preexistingPart1 is not KitchenCupboardWorktopItem)
            {
                KitchenCupboardWorktopItem newWorktop = new KitchenCupboardWorktopItem();
                if (preexistingPart1 is IHasModelPartColour colourComponent) newWorktop.ColourData.Colour = colourComponent.ColourData.Colour;
                slots[1].SetPart(newWorktop);
            }
            slots[0].Name = "Unit";
            slots[1].Name = "Worktop";
            slots[2].Name = "Door";
        }
    }
}
