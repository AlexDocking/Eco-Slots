using Eco.Core.Tests;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Utils;
using Parts.Kitchen;
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
            InventorySlot slot = TestUtility.CreateInventorySlot();
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
            InventorySlot slot = TestUtility.CreateInventorySlot();
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
            InventorySlot slot = TestUtility.CreateInventorySlot();
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
        public static void ShouldSetPartCorrectly()
        {
            ISlot slot = TestUtility.CreateSlot();
            //WorldObject worldObject = new TestWorldObject();
            //PartsContainer partsContainer = new PartsContainer();
            //slot.Initialize(worldObject, partsContainer);

            TestPart item = new TestPart();
            int calls = 0;
            slot.NewPartInSlotEvent.Add(() => calls += 1);
            DebugUtils.AssertEquals(null, slot.Part, "Slot should have no part until set");

            slot.SetPart(null);
            DebugUtils.AssertEquals(null, slot.Part, "Slot should have no part until set");
            DebugUtils.AssertEquals(0, calls, "Slot should not trigger event when slot is not changed");

            slot.SetPart(item);
            slot.SetPart(item);
            DebugUtils.AssertEquals(item, slot.Part, "Slot should have have set part");
            DebugUtils.AssertEquals(1, calls, "Slot should trigger event once when slot is changed");

            calls = 0;
            slot.SetPart(null);
            DebugUtils.AssertEquals(null, slot.Part, "Slot should have no part");
            DebugUtils.AssertEquals(1, calls, "Slot should trigger event once when slot is changed");
        }

        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldChangeAnimatorStateColours()
        {
            //create an object which sets up no new slots or parts of its own
            TestWorldObject worldObject = new TestWorldObject();

            //create a new parts container with a part which has a colour
            KitchenBaseCabinetBoxItem box = new KitchenBaseCabinetBoxItem();
            PartsContainer partsContainer = new PartsContainer();
            Color targetColour = Color.Orange;
            box.ColourData.Colour = targetColour;
            ISlot slot = TestUtility.CreateSlot();
            partsContainer.TryAddSlot(slot, box);

            TestWorldObjectItem.Migrator = new TestPartsContainerMigrator(partsContainer);

            worldObject.CreationItem = new TestWorldObjectItem();

            worldObject.InitializeForTest();

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
            TestWorldObject worldObject = new TestWorldObject();

            //create a new parts container with a part which has a colour
            PartsContainer partsContainer = new PartsContainer();
            ISlot slot = TestUtility.CreateSlot();
            partsContainer.TryAddSlot(slot, null);

            TestWorldObjectItem.Migrator = new TestPartsContainerMigrator(partsContainer);
            worldObject.CreationItem = new TestWorldObjectItem();

            worldObject.InitializeForTest();

            KitchenCabinetFlatDoorItem door = new KitchenCabinetFlatDoorItem();
            worldObject.AnimatedStates.TryGetValue("Flat Door", out object flatDoorEnabled);
            DebugUtils.AssertEquals(false, flatDoorEnabled, "Should not tell the model to enable the door when it is not installed in the container");

            slot.TryAddPart(door);

            worldObject.AnimatedStates.TryGetValue("Flat Door", out flatDoorEnabled);
            DebugUtils.AssertEquals(true, flatDoorEnabled, "Should tell the model to enable the door when it is installed in the container");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTrackPartChangesBeforeInitialization()
        {
            ISlot slot = TestUtility.CreateSlot();
            int calls = 0;
            slot.NewPartInSlotEvent.Add(() => calls += 1);
            slot.TryAddPart(new TestPart());
            DebugUtils.AssertEquals(typeof(TestPart), slot.Part?.GetType(), "Did not set part correctly");
            DebugUtils.AssertEquals(1, calls, "Did not trigger event exactly once");

            calls = 0;
            slot.SetPart(null);
            DebugUtils.AssertEquals(null, slot.Part?.GetType(), "Did not set part correctly");
            DebugUtils.AssertEquals(1, calls, "Did not trigger event exactly once");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldSetOwnerWorldObject()
        {
            InventorySlot slot = TestUtility.CreateInventorySlot();
            DebugUtils.Assert(!slot.Inventory.Owner.TryGetObject(out _), "Slot inventory should have no owner at creation");
            WorldObject parent = new TestWorldObject();
            parent.InitializeForTest();

            slot.Initialize(parent, PartsContainerFactory.Create());
            DebugUtils.Assert(slot.Inventory.Owner.TryGetObject(out WorldObject actualParent), "Slot inventory should have an owner after initialization");
            DebugUtils.AssertEquals(parent, actualParent, "Slot inventory should have the correct owner after initialization");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldBeAbleToReceivePartTypesBasedOnSlotDefinition()
        {
            ISlotDefinition slotDefinition = new DefaultInventorySlotDefinition()
            {
                AllowedItemTypes = new[] { typeof(TestPart) }
            };
            ISlot slot = TestUtility.CreateAndInitializeInventorySlot(slotDefinition);
            IPart validPart = new TestPart();
            DebugUtils.Assert(slot.CanAcceptPart(validPart), "Slot should accept part of valid type as set by the slot definition");
            IPart invalidPart = new TestPart2();
            DebugUtils.Assert(!slot.CanAcceptPart(invalidPart), "Slot should not accept a part not in the list of allowable types as set in the slot definition");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldBeAbleToRemovePartIfOptionalInSlotDefinition()
        {
            ISlotDefinition optionalSlotDefinition = new DefaultInventorySlotDefinition()
            {
                Optional = true
            };
            ISlot optionalSlot = TestUtility.CreateAndInitializeInventorySlot(optionalSlotDefinition);
            IPart part = new TestPart();
            DebugUtils.Assert(!optionalSlot.CanRemovePart(), "If slot has no part then nothing can be removed");
            optionalSlot.SetPart(part);
            DebugUtils.Assert(optionalSlot.CanRemovePart(), "Slot is optional so the part should be allowed to be removed");

            ISlotDefinition nonOptionalSlotDefinition = new DefaultInventorySlotDefinition()
            {
                Optional = false
            };
            ISlot nonOptionalSlot = TestUtility.CreateAndInitializeInventorySlot(nonOptionalSlotDefinition);
            DebugUtils.Assert(!nonOptionalSlot.CanRemovePart(), "If slot has no part then nothing can be removed");
            nonOptionalSlot.SetPart(part);
            DebugUtils.Assert(!nonOptionalSlot.CanRemovePart(), "Slot is not optional so the part should be not allowed to be removed");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerEventWhenStorageRequirementStatusChanges()
        {
            IPartsContainer partsContainer = PartsContainerFactory.Create(new PartsContainerSchema(new[]
            {
                new DefaultInventorySlotDefinition()
                {
                    RequiresEmptyStorageToChangePart = true,
                    Optional = true
                }
            }));
            partsContainer.TryAddSlot(TestUtility.CreateSlot(), new TestPart());

            Inventory storage = new LimitedInventory(10);

            InventorySlot slot = partsContainer.Slots[0] as InventorySlot;
            if (!DebugUtils.Assert(slot is not null, "Created slot was not InventorySlot")) return;

            slot.RequireEmptyStorageSlotStatus = new RequireEmptyStorageSlotStatus(storage);
            slot.Initialize(new HewnDoorObject(), null);

            int calls = 0;
            slot.SlotStatusChanged.Add(s => {
                calls += 1;
                DebugUtils.AssertEquals(slot, s, "Event called with wrong slot parameter");
            });
            storage.AddItems(typeof(CornItem), 1);
            DebugUtils.AssertEquals(1, calls, "Adding an item so that the storage becomes non-empty should have triggered a change of status");

            calls = 0;
            storage.AddItems(typeof(FirSeedItem), 1);
            DebugUtils.AssertEquals(0, calls, "Adding an item to the non-empty storage should not have triggered a change of status");

            calls = 0;
            storage.RemoveItems(typeof(FirSeedItem), 1);
            DebugUtils.AssertEquals(0, calls, "Removing an item while the storage is still not empty should not have triggered a change of status");

            calls = 0;
            storage.RemoveItems(typeof(CornItem), 1);
            DebugUtils.AssertEquals(1, calls, "Removing an item so that the storage becomes empty should have triggered a change of status");
        }
    }
}