using Eco.Core.Utils;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Mods.TechTree;
using Eco.Shared.IoC;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Parts.Migration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts.Tests
{
    public static class TestUtility
    {
        public static ISlot CreateSlot() => CreateInventorySlot();
        public static InventorySlot CreateInventorySlot() => CreateInventorySlot(new RegularSlotDefinition());
        public static InventorySlot CreateInventorySlot(ISlotDefinition slotDefinition)
        {
            return new InventorySlot(slotDefinition);
        }

        public static ISlot CreateSlot(ISlotDefinition slotDefinition) => CreateInventorySlot(slotDefinition);
        public static WorldObject CreateWorldObject(IPartsContainer existingPartsContainer, IPartsContainerMigrator migrator = null)
        {
            WorldObject worldObject = new TestWorldObject();
            typeof(WorldObjectManager).GetMethod("InsertWorldObject", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke((WorldObjectManager)ServiceHolder<IWorldObjectManager>.Obj, new object[] { worldObject });
            ItemPersistentData itemPersistentData = new ItemPersistentData();
            itemPersistentData.SetPersistentData<PartsContainerComponent>(existingPartsContainer);
            worldObject.CreationItem = new TestWorldObjectItem() { PersistentData = itemPersistentData };
            worldObject.DoInitializationSteps();
            worldObject.GetComponent<PartsContainerComponent>().Migrator = migrator ?? new TestPartsContainerMigrator(existingPartsContainer);
            worldObject.FinishInitialize();
            worldObject.Components.ForEach(x => x.PostInitialize());
            typeof(WorldObject).GetProperty(nameof(WorldObject.IsInitialized)).SetValue(worldObject, true);
            return worldObject;
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
            //In order for inventories to get a WorldObjectHandle the WorldObject must be registered with the WorldObjectManager
            typeof(WorldObjectManager).GetMethod("InsertWorldObject", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke((WorldObjectManager)ServiceHolder<IWorldObjectManager>.Obj, new object[] { worldObject });

            worldObject.DoInitializationSteps();
            worldObject.FinishInitialize();
            worldObject.Components.ForEach(x => x.PostInitialize());
            typeof(WorldObject).GetProperty(nameof(WorldObject.IsInitialized)).SetValue(worldObject, true);
        }
    }
    public class TestColouredPart : IHasModelPartColour
    {
        public ModelPartColouring ColourData { get; internal set; } = new ModelPartColouring();

        public string DisplayName => "Test name";

        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();
    }
    [Serialized]
    [Category("Hidden")]
    public class TestPart : Item, IPart
    {
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();

        string IPart.DisplayName => "Test Part";
    }
    [Serialized]
    [Category("Hidden")]
    public class TestPart2 : Item, IPart
    {
        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();

        string IPart.DisplayName => "Test Part 2";
    }
    [Serialized]
    public class FakePartsContainer : IPartsContainer
    {
        public IReadOnlyList<IPart> Parts => new List<IPart>();
        public IReadOnlyList<ISlot> Slots { get; set; } = new List<ISlot>();
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
    public class FakePartsContainerFactory : IPartsContainerFactory
    {
        public FakePartsContainer Instance { get; set; }
        public IPartsContainer Create() => Instance;
    }
    [Serialized]
    [LocCategory("Hidden")]
    public class TestWorldObjectItem : WorldObjectItem<TestWorldObject>, IPartsContainerWorldObject, IPersistentData
    {
        public ItemPersistentData persistentData;
        public object PersistentData { get => persistentData; set => persistentData = value as ItemPersistentData; }
        public static IPartsContainerMigrator Migrator { get; set; }

        public IPartsContainerMigrator GetPartsContainerMigrator() => Migrator;
    }
    public class TestPartsContainerMigrator : IPartsContainerMigrator
    {
        private readonly IPartsContainer subsitute;
        public TestPartsContainerMigrator(IPartsContainer subsitute)
        {
            this.subsitute = subsitute;
        }
        public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
        {
            if (existingContainer.Slots.Any()) subsitute?.TryAddSlot(TestUtility.CreateSlot(), existingContainer.Parts.FirstOrDefault());
            return subsitute;
        }
    }
    [Serialized]
    [LocCategory("Hidden")]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(ModelReplacerComponent))]
    [RequireComponent(typeof(ModelPartColourComponent))]
    [RequireComponent(typeof(PublicStorageComponent))]
    public class TestWorldObject : WorldObject, IRepresentsItem
    {
        public Type RepresentedItemType => typeof(TestWorldObjectItem);

        protected override void Initialize()
        {
            base.Initialize();
            GetComponent<PublicStorageComponent>().Initialize(1, 10000);
        }
    }
    
}