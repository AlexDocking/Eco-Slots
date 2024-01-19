using Eco.Core.Tests;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using KitchenUnits;
using Parts.Migration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Parts.Tests.TestPartsContainer;

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
            Slot slot = new Slot() { Name = "Box" };
            KitchenBaseCabinetBoxItem part = new KitchenBaseCabinetBoxItem();
            existingPartsContainer.AddPart(slot, part);

            //set up existingPartsContainer as the persistent data
            ItemPersistentData itemPersistentData = new ItemPersistentData();
            itemPersistentData.Entries.Add(typeof(PartsContainerComponent), existingPartsContainer);
            worldObject.CreationItem = new TestWorldObjectItem() { persistentData = itemPersistentData };

            worldObject.InitializeForTest();

            //should restore existingPartsContainer as persistent data, then migrate its contents to the migratedPartsContainer instance
            PartsContainerComponent partsContainerComponent = worldObject.GetOrCreateComponent<PartsContainerComponent>();


            DebugUtils.AssertEquals(migratedPartsContainer, partsContainerComponent?.PartsContainer, "Should have used migration schema to set the new parts container");
            DebugUtils.AssertEquals(part, partsContainerComponent?.PartsContainer?.Parts?.FirstOrDefault(), "Should have passed in existing parts container to migration schema");
        }
        [Serialized]
        public class FakePartsContainer : IPartsContainer
        {
            public IReadOnlyList<IPart> Parts => new List<IPart>();
            public IReadOnlyList<Slot> Slots => new List<Slot>();
            public ISlotRestrictionManager SlotRestrictionManager { get; set; }
            public ThreadSafeAction<Slot> NewPartInSlotEvent { get; } = new ThreadSafeAction<Slot>();
            int id;
            public ref int ControllerID => ref id;
            public event PropertyChangedEventHandler PropertyChanged;
            public void AddPart(Slot slot, IPart part) { }

            /// <summary>
            /// Count the number of times Initialize is called
            /// </summary>
            public int NumberOfInitializeCalls { get; private set; }
            public void Initialize(WorldObject worldObject)
            {
                NumberOfInitializeCalls += 1;
            }

            public void RemovePart(Slot slot) { }
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

    public class TestWorldObjectItem : WorldObjectItem<TestWorldObject>, IPersistentData
    {
        public ItemPersistentData persistentData;
        public object PersistentData { get => persistentData; set => persistentData = value as ItemPersistentData; }
    }
    [Serialized]
    [LocCategory("Hidden")]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(ModelReplacerComponent))]
    [RequireComponent(typeof(ModelPartColourComponent))]
    public class TestWorldObject : WorldObject, IPartsContainerWorldObject
    {
        public IPartsContainerSchema Schema { get; set; }
        public IPartsContainerSchema GetPartsContainerSchema() => Schema;
    }
    public class TestPartsContainerSchema : IPartsContainerSchema
    {
        private readonly IPartsContainer subsitute;
        public TestPartsContainerSchema(IPartsContainer subsitute)
        {
            this.subsitute = subsitute;
        }
        public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
        {
            subsitute?.AddPart(new Slot(), existingContainer.Parts.FirstOrDefault());
            return subsitute;
        }
    }
    public static class TestWorldObjectExtensions
    {
        /// <summary>
        /// Initialize a test-only world object without placing it in the world
        /// </summary>
        /// <param name="worldObject"></param>
        public static void InitializeForTest(this WorldObject worldObject)
        {
            worldObject.DoInitializationSteps();
            worldObject.FinishInitialize();
            worldObject.Components.ForEach(x => x.PostInitialize());
            typeof(WorldObject).GetProperty(nameof(WorldObject.IsInitialized)).SetValue(worldObject, true);
        }
    }
}
