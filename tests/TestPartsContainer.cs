using Eco.Core.Tests;
using Eco.Core.Utils;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Items.Recipes;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Mods.TechTree;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Parts.Kitchen;
using Parts.Migration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            int calls = 0;
            Action<IPartsContainer> callback = container =>
                        {
                            calls += 1;
                            DebugUtils.AssertEquals(partsContainer, container);
                        };
            PartsContainer.PartsContainerChangedEventGlobal.Add(callback);

            partsContainer.Initialize(worldObject);
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
        public static void ShouldUseSchemaToMigratePartsContainer()
        {
            PartsContainer migratedPartsContainer = new PartsContainer();

            WorldObject worldObject = new TestWorldObject() { Schema = new TestPartsContainerSchema(migratedPartsContainer) };
            
            IPartsContainer existingPartsContainer = new PartsContainer();
            ISlot slot = TestUtility.CreateSlot(new RegularSlotDefinition() { Name = "Box" });
            KitchenBaseCabinetBoxItem part = new KitchenBaseCabinetBoxItem();
            existingPartsContainer.Initialize(worldObject);

            existingPartsContainer.TryAddSlot(slot, part);
            if (!DebugUtils.AssertEquals(part.GetType(), slot.Part?.GetType(), "Could not set part")) return;

            //set up existingPartsContainer as the persistent data
            ItemPersistentData itemPersistentData = new ItemPersistentData();
            itemPersistentData.Entries.Add(typeof(PartsContainerComponent), existingPartsContainer);
            worldObject.CreationItem = new TestWorldObjectItem() { persistentData = itemPersistentData };

            worldObject.InitializeForTest();

            //Should restore existingPartsContainer to the PartsContainerComponent as persistent data,
            //then migrate its contents to the migratedPartsContainer instance
            PartsContainerComponent partsContainerComponent = worldObject.GetOrCreateComponent<PartsContainerComponent>();


            DebugUtils.AssertEquals(migratedPartsContainer, partsContainerComponent?.PartsContainer, "Should have used migration schema to set the new parts container");
            DebugUtils.AssertEquals(part, partsContainerComponent?.PartsContainer?.Parts?.FirstOrDefault(), "Should have passed in existing parts container to migration schema");
        }
        [Serialized]
        public class FakePartsContainer : IPartsContainer
        {
            public IReadOnlyList<IPart> Parts => new List<IPart>();
            public IReadOnlyList<ISlot> Slots { get; set; } = new List<ISlot>();
            public ISlotRestrictionManager SlotRestrictionManager { get; set; }
            public ThreadSafeAction<ISlot> NewPartInSlotEvent { get; } = new ThreadSafeAction<ISlot>();
            int id;
            public ref int ControllerID => ref id;
            public event PropertyChangedEventHandler PropertyChanged;
            public bool TryAddSlot(ISlot slot, IPart part) => true;

            /// <summary>
            /// Count the number of times Initialize is called
            /// </summary>
            public int NumberOfInitializeCalls { get; private set; }
            public void Initialize(WorldObject worldObject)
            {
                NumberOfInitializeCalls += 1;
            }

            public void RemovePart(ISlot slot) { }
        }
        
        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldInitializePartsContainerFromPersistentData()
        {
            FakePartsContainer fakePartsContainer = new FakePartsContainer();

            WorldObject worldObject = new TestWorldObject() { Schema = new TestPartsContainerSchema(null) };
            //set up fakePartsContainer as the persistent data
            ItemPersistentData itemPersistentData = new ItemPersistentData();
            itemPersistentData.Entries.Add(typeof(PartsContainerComponent), fakePartsContainer);
            worldObject.CreationItem = new TestWorldObjectItem() { persistentData = itemPersistentData };

            //should now restore persistent data
            worldObject.InitializeForTest();

            DebugUtils.AssertEquals(1, fakePartsContainer.NumberOfInitializeCalls, "Should have initialized parts container from persistent data during Initialize()");
        }
        public class FakePartsContainerFactory : IPartsContainerFactory
        {
            public FakePartsContainer Instance { get; set; }
            public IPartsContainer Create() => Instance;
        }

        [CITest]
        [ChatCommand("Test", ChatAuthorizationLevel.Developer)]
        public static void ShouldInitializeNewPartsContainerWhenThereIsNoPersistentData()
        {
            FakePartsContainer fakePartsContainer = new FakePartsContainer();
            FakePartsContainerFactory fakePartsContainerFactory = new FakePartsContainerFactory() { Instance = fakePartsContainer };
            PartsContainerFactory.Factory = fakePartsContainerFactory;
            WorldObject worldObject = new TestWorldObject();
            worldObject.InitializeForTest();

            PartsContainerFactory.Factory = new DefaultPartsContainerFactory();

            DebugUtils.AssertEquals(1, fakePartsContainer.NumberOfInitializeCalls, "Should have initialized new parts container given by factory");
        }
    }
    [Serialized]
    [LocCategory("Hidden")]
    public class TestWorldObjectItem : WorldObjectItem<TestWorldObject>, IPersistentData
    {
        public ItemPersistentData persistentData;
        public object PersistentData { get => persistentData; set => persistentData = value as ItemPersistentData; }
    }
    
}
