using Eco.Core.Tests;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using KitchenUnits;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts.Tests
{
    [ChatCommandHandler]
    public static class TestPartsContainer
    {
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldParentSlotToPartsContainer()
        {
            PartsContainer partsContainer = new PartsContainer();
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.AddPart(slot, null);
            partsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(partsContainer, slot.PartsContainer, $"Did not set parent slot to {nameof(PartsContainer)} correctly");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerSlotChangedEvent()
        {
            PartsContainer partsContainer = new PartsContainer();
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.AddPart(slot, null);
            partsContainer.Initialize(worldObject);

            int calls = 0;
            Action<Slot> callback = s =>
                        {
                            calls += 1;
                            DebugUtils.AssertEquals(slot, s);
                        };
            partsContainer.NewPartInSlotEvent.Add(callback);

            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            slot.Inventory.AddItem(box);

            partsContainer.NewPartInSlotEvent.Remove(callback);
            DebugUtils.AssertEquals(1, calls, $"Did not trigger event on {nameof(PartsContainer)} when item was added to slot");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerGlobalEventWhenSlotChanges()
        {
            PartsContainer partsContainer = new PartsContainer();
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.AddPart(slot, null);
            partsContainer.Initialize(worldObject);

            int calls = 0;
            Action<IPartsContainer> callback = container =>
                        {
                            calls += 1;
                            DebugUtils.AssertEquals(partsContainer, container);
                        };
            PartsContainer.PartsContainerChangedEventGlobal.Add(callback);

            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            slot.Inventory.AddItem(box);
            DebugUtils.AssertEquals(1, calls, $"Should have fired global event on {nameof(PartsContainer)} when item was added to slot");

            calls = 0;
            box.ColourData.Colour = Color.Orange;
            PartsContainer.PartsContainerChangedEventGlobal.Remove(callback);

            DebugUtils.AssertEquals(1, calls, $"Should have fired global event on {nameof(PartsContainer)} when item colour changed");
        }
    }
}
