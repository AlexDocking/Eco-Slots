using Eco.Core.Tests;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Utils;
using Parts.Kitchen;
using System;
using System.Linq;

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
            ISlot slot = TestUtility.CreateSlot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.TryAddSlot(slot, null);
            partsContainer.Initialize(worldObject);

            DebugUtils.AssertEquals(partsContainer, slot.PartsContainer, $"Did not set parent slot to {nameof(PartsContainer)} correctly");
        }
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldTriggerSlotChangedEvent()
        {
            PartsContainer partsContainer = new PartsContainer();
            InventorySlot slot = TestUtility.CreateInventorySlot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.TryAddSlot(slot, null);
            partsContainer.Initialize(worldObject);

            int calls = 0;
            Action<ISlot> callback = s =>
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
            InventorySlot slot = TestUtility.CreateInventorySlot();
            WorldObject worldObject = new KitchenCupboardObject();
            partsContainer.TryAddSlot(slot, null);
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

        
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldUseMigratorToMigratePartsContainer()
        {
            PartsContainer migratedPartsContainer = new PartsContainer();

            
            IPartsContainer existingPartsContainer = new PartsContainer();
            ISlot slot = TestUtility.CreateSlot(new RegularSlotDefinition() { Name = "Box" });
            KitchenBaseCabinetBoxItem part = new KitchenBaseCabinetBoxItem();
            existingPartsContainer.TryAddSlot(slot, part);
            //existingPartsContainer.Initialize(worldObject);
            if (!DebugUtils.AssertEquals(part, existingPartsContainer.Parts.FirstOrDefault(), "Could not set part")) return;

            //set up existingPartsContainer as the persistent data
            //ItemPersistentData itemPersistentData = new ItemPersistentData();
            //itemPersistentData.Entries.Add(typeof(PartsContainerComponent), existingPartsContainer);
            //TestWorldObjectItem.Migrator = new TestPartsContainerMigrator(existingPartsContainer);
            //worldObject.CreationItem = new TestWorldObjectItem() { persistentData = itemPersistentData };

            //worldObject.InitializeForTest();
            WorldObject worldObject = TestUtility.CreateWorldObject(existingPartsContainer, new TestPartsContainerMigrator(migratedPartsContainer));

            //Should restore existingPartsContainer to the PartsContainerComponent as persistent data,
            //then migrate its contents to the migratedPartsContainer instance
            PartsContainerComponent partsContainerComponent = worldObject.GetOrCreateComponent<PartsContainerComponent>();


            DebugUtils.AssertEquals(migratedPartsContainer, partsContainerComponent?.PartsContainer, "Should have used migrator to set the new parts container");
            DebugUtils.AssertEquals(part, partsContainerComponent?.PartsContainer?.Parts?.FirstOrDefault(), "Should have passed in existing parts container to migrator");
        }

        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldInitializePartsContainerFromPersistentData()
        {
            FakePartsContainer fakePartsContainer = new FakePartsContainer();

            WorldObject worldObject = new TestWorldObject();
            //set up fakePartsContainer as the persistent data
            ItemPersistentData itemPersistentData = new ItemPersistentData();
            itemPersistentData.Entries.Add(typeof(PartsContainerComponent), fakePartsContainer);
            TestWorldObjectItem.Migrator = new TestPartsContainerMigrator(fakePartsContainer);
            worldObject.CreationItem = new TestWorldObjectItem() { persistentData = itemPersistentData };

            //should now restore persistent data
            worldObject.InitializeForTest();

            DebugUtils.AssertEquals(1, fakePartsContainer.NumberOfInitializeCalls, "Should have initialized parts container from persistent data during Initialize()");
        }

        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldInitializeNewPartsContainerWhenThereIsNoPersistentData()
        {
            FakePartsContainer fakePartsContainer = new FakePartsContainer();
            //FakePartsContainerFactory fakePartsContainerFactory = new FakePartsContainerFactory() { Instance = fakePartsContainer };
            //PartsContainerFactory.Factory = fakePartsContainerFactory;
            TestUtility.CreateWorldObject(fakePartsContainer);
            //worldObject.InitializeForTest();
            //PartsContainerFactory.Factory = new DefaultPartsContainerFactory();

            DebugUtils.AssertEquals(1, fakePartsContainer.NumberOfInitializeCalls, "Should have initialized new parts container given by factory");
        }
    }
    
    
}
