using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Objects;
using Eco.Shared.IoC;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Linq;

namespace Parts.Tests
{
    public static class TestUtility
    {
        public static ISlot CreateSlot()
        {
            return new InventorySlot();
        }
        public static InventorySlot CreateInventorySlot()
        {
            return new InventorySlot();
        }
        public static InventorySlot CreateInventorySlot(ISlotDefinition slotDefinition)
        {
            return new InventorySlot(slotDefinition);
        }

        public static ISlot CreateSlot(ISlotDefinition slotDefinition) => CreateInventorySlot(slotDefinition);
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
            subsitute?.TryAddSlot(TestUtility.CreateSlot(), existingContainer.Parts.FirstOrDefault());
            return subsitute;
        }
    }
    [Serialized]
    [LocCategory("Hidden")]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(ModelReplacerComponent))]
    [RequireComponent(typeof(ModelPartColourComponent))]
    [RequireComponent(typeof(PublicStorageComponent))]
    public class TestWorldObject : WorldObject, IPartsContainerWorldObject
    {
        public IPartsContainerSchema Schema { get; set; }
        public IPartsContainerSchema GetPartsContainerSchema() => Schema;
        protected override void Initialize()
        {
            base.Initialize();
            GetComponent<PublicStorageComponent>().Initialize(1, 10000);
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
}