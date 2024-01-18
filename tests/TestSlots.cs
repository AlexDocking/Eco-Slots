﻿using Eco.Core.Tests;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Utils;
using KitchenUnits;
using System;
using System.Linq;

namespace Parts.Tests
{
    [ChatCommandHandler]
    public static class TestSlots
    {
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldUpdatePartWhenInventoryChanges()
        {
            Slot slot = new Slot();
            DebugUtils.AssertEquals(null, slot.Part, "Slot should be empty at creation");
            DebugUtils.AssertEquals(null, slot.Inventory.Stacks.First().Item, "Slot should be empty until given a part");

            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);
            DebugUtils.AssertEquals(null, slot.Part, "Slot should be empty until given a part");
            DebugUtils.AssertEquals(null, slot.Inventory.Stacks.First().Item, "Slot should be empty until given a part");

            KitchenBaseCabinetBoxItem item = new KitchenBaseCabinetBoxItem();
            slot.Inventory.AddItem(item);
            DebugUtils.AssertEquals(item, slot.Part, "Slot should know its new part when inventory changes");
            DebugUtils.AssertEquals(item, slot.Inventory.Stacks.First().Item, "Item should have gone in the first stack");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerPartChangedEventWhenInventoryChanges()
        {
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            int calls = 0;
            slot.NewPartInSlotEvent.Add(() => calls += 1);
            KitchenBaseCabinetBoxItem item = new KitchenBaseCabinetBoxItem();
            slot.Inventory.AddItem(item);
            DebugUtils.AssertEquals(1, calls, "Slot should trigger event when inventory changes");

            calls = 0;
            slot.Inventory.Clear();
            DebugUtils.AssertEquals(1, calls, "Slot should trigger event when inventory changes");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerPartPropertyChangedEventWhenPartPropertyChanges()
        {
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            KitchenBaseCabinetBoxItem item = new KitchenBaseCabinetBoxItem();
            int calls = 0;
            slot.PartPropertyChangedEvent.Add((s, part, property) =>
            {
                calls += 1;
                DebugUtils.AssertEquals(slot, s);
                DebugUtils.AssertEquals(part, item);
                DebugUtils.AssertEquals(item.ColourData, property);
            });
            slot.Inventory.AddItem(item);
            DebugUtils.AssertEquals(0, calls, "Slot should not trigger part property change event when inventory changes");
            item.ColourData.Colour = Color.Orange;
            DebugUtils.AssertEquals(1, calls, "Slot should trigger part property change event when part colour changes");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldOnlyAcceptCorrectItems()
        {
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetTypeRestriction(slot, new Type[] { typeof(KitchenCupboardWorktopItem) });

            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            Result attemptedInvalidResult = slot.Inventory.TryAddItem(box);
            DebugUtils.Assert(attemptedInvalidResult.Failed, "Slot inventory should not accept wrong item");
            DebugUtils.AssertEquals(null, slot.Part, "Slot part should not have changed");

            KitchenCupboardWorktopItem worktop = new KitchenCupboardWorktopItem();
            Result attemptedValidResult = slot.Inventory.TryAddItem(worktop);
            DebugUtils.Assert(!attemptedValidResult.Failed, "Slot inventory should accept correct item");
            DebugUtils.AssertEquals(worktop, slot.Part, "Slot part should have changed");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldNotAllowRemovalUnlessOptional()
        {
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            slotRestrictionManager.SetOptional(slot, false);

            bool canRemovePart = slot.Inventory.GetMaxPickup(Item.Get<KitchenBaseCabinetBoxItem>(), 1).Val > 0;
            DebugUtils.Assert(!canRemovePart, "Slot should not allow part to be removed when it is mandatory");

            slotRestrictionManager.SetOptional(slot, true);
            canRemovePart = slot.Inventory.GetMaxPickup(Item.Get<KitchenBaseCabinetBoxItem>(), 1).Val > 0;
            DebugUtils.Assert(canRemovePart, "Slot should allow part to be removed when it is optional");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldOnlyAllowCertainTypesOfPartsIntoInventory()
        {
            Slot slot = new Slot();
            WorldObject worldObject = new KitchenCupboardObject();
            PartsContainer partsContainer = new PartsContainer();
            slot.Initialize(worldObject, partsContainer);

            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();

            bool acceptsAnyPart = slot.Inventory.GetMaxAccepted(Item.Get<KitchenCupboardWorktopItem>(), 0).Val > 0;
            DebugUtils.Assert(acceptsAnyPart, "Slot should accept any part until set otherwise");

            slotRestrictionManager.SetTypeRestriction(slot, new Type[] { typeof(KitchenBaseCabinetBoxItem) });

            bool acceptsValidPart = slot.Inventory.GetMaxAccepted(Item.Get<KitchenBaseCabinetBoxItem>(), 0).Val > 0;
            DebugUtils.Assert(acceptsValidPart, "Slot should allow in items on the whitelist");

            bool acceptsInvalidPart = slot.Inventory.GetMaxAccepted(Item.Get<KitchenCupboardWorktopItem>(), 0).Val > 0;
            DebugUtils.Assert(!acceptsInvalidPart, "Slot should not allow in items not on the whitelist");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldChangeAnimatorStateColours()
        {
            //create an object which sets up no new slots or parts of its own
            WorldObject worldObject = new LimestoneOtterStatueObject();
            PartsContainerComponent partsContainerComponent = worldObject.GetOrCreateComponent<PartsContainerComponent>();
            ModelPartColourComponent componentToSetColourAnimatorStates = worldObject.GetOrCreateComponent<ModelPartColourComponent>();

            //create a new parts container with a part which has a colour
            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            PartsContainer partsContainer = new PartsContainer();
            Color targetColour = Color.Orange;
            box.ColourData.Colour = targetColour;
            Slot slot = new Slot();
            partsContainer.AddPart(slot, box);
            Item t;
            partsContainerComponent.PartsContainer = partsContainer;
            partsContainer.Initialize(worldObject);
            componentToSetColourAnimatorStates.Initialize();
            componentToSetColourAnimatorStates.PostInitialize();

            //check the animated states for colour was set
            worldObject.AnimatedStates.TryGetValue(box.ColourData.ModelName + "-Red", out object red);
            worldObject.AnimatedStates.TryGetValue(box.ColourData.ModelName + "-Green", out object green);
            worldObject.AnimatedStates.TryGetValue(box.ColourData.ModelName + "-Blue", out object blue);
            DebugUtils.AssertEquals(targetColour.R, red, "Should have set a colour for the box");
            DebugUtils.AssertEquals(targetColour.G, green, "Should have set a colour for the box");
            DebugUtils.AssertEquals(targetColour.B, blue, "Should have set a colour for the box");

            targetColour = Color.BlueGrey;
            box.ColourData.Colour = targetColour;
            worldObject.AnimatedStates.TryGetValue(box.ColourData.ModelName + "-Red", out red);
            worldObject.AnimatedStates.TryGetValue(box.ColourData.ModelName + "-Green", out green);
            worldObject.AnimatedStates.TryGetValue(box.ColourData.ModelName + "-Blue", out blue);
            DebugUtils.AssertEquals(targetColour.R, red, "Should have updated the colour for the box");
            DebugUtils.AssertEquals(targetColour.G, green, "Should have updated the colour for the box");
            DebugUtils.AssertEquals(targetColour.B, blue, "Should have updated the colour for the box");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldChangeEnabledModelParts()
        {
            //create an object which sets up no new slots or parts of its own
            WorldObject worldObject = new LimestoneOtterStatueObject();
            PartsContainerComponent partsContainerComponent = worldObject.GetOrCreateComponent<PartsContainerComponent>();
            ModelReplacerComponent componentToSetEnabledModelParts = worldObject.GetOrCreateComponent<ModelReplacerComponent>();

            //create a new parts container with a part which has a colour
            PartsContainer partsContainer = new PartsContainer();
            Slot slot = new Slot();
            partsContainer.AddPart(slot, null);

            partsContainerComponent.PartsContainer = partsContainer;
            partsContainer.Initialize(worldObject);
            componentToSetEnabledModelParts.Initialize();
            componentToSetEnabledModelParts.PostInitialize();

            KitchenCabinetFlatDoorItem door = new KitchenCabinetFlatDoorItem();
            worldObject.AnimatedStates.TryGetValue("Flat Door", out object flatDoorEnabled);
            DebugUtils.AssertEquals(false, flatDoorEnabled, "Should not tell the model to enable the door when it is not installed in the container");

            slot.TryAddPart(door);

            worldObject.AnimatedStates.TryGetValue("Flat Door", out flatDoorEnabled);
            DebugUtils.AssertEquals(true, flatDoorEnabled, "Should tell the model to enable the door when it is installed in the container");
        }
    }
}