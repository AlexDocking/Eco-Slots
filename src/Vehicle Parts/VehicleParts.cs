using Eco.Core.Controller;
using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using KitchenUnits;
using Parts;
using Parts.Migration;
using Parts.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Parts.Vehicles
{
    public interface IHasStorage
    {
        public int BaseNumberOfSlots { get; }
        public int BaseWeightLimit { get; }
    }
    public interface ICustomStorageSize : IController, INotifyPropertyChanged, IPartProperty
    {
        public int NumberOfAdditionalSlots { get; }
    }
    public class CustomPublicStorageSize : ICustomStorageSize, IPartProperty
    {
        public int NumberOfAdditionalSlots { get; init; }

        #region IController
        private int id;
        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    public interface IHasCustomStorageSize
    {
        public ICustomStorageSize StorageSizeModifier { get; set; }
    }
    public class TruckSchema : IPartsContainerSchema
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
            slotRestrictionManager.SetTypeRestriction(slots[0], new[] { typeof(StandardTruckBedItem), typeof(BigTruckBedItem) });
            slotRestrictionManager.SetOptional(slots[0], true);

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
                slots[0].Inventory.Stacks.First().Item = newBed;
            }
            slots[0].Name = "Truck Bed";
        }
    }
    [Serialized]
    public class StandardTruckBedItem : Item, IPart, IHasCustomStorageSize
    {
        private ICustomStorageSize storageSizeModifier;

        public StandardTruckBedItem() : base()
        {
            StorageSizeModifier = new CustomPublicStorageSize() { NumberOfAdditionalSlots = 5 };
        }
        public ICustomStorageSize StorageSizeModifier { get => storageSizeModifier; set => this.SetProperty(value, ref storageSizeModifier); }

        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();

        string IPart.DisplayName => "Standard Truck Bed";
    }
    [Serialized]
    public class BigTruckBedItem : Item, IPart, IHasCustomStorageSize
    {
        private ICustomStorageSize storageSizeModifier;

        public BigTruckBedItem() : base()
        {
            StorageSizeModifier = new CustomPublicStorageSize() { NumberOfAdditionalSlots = 20 };
        }
        public ICustomStorageSize StorageSizeModifier { get => storageSizeModifier; set => this.SetProperty(value, ref storageSizeModifier); }

        public ThreadSafeAction<IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<IPart, IPartProperty>();

        string IPart.DisplayName => "Big Truck Bed";
    }
    public class StorageSizeSetterViewController : IController, INotifyPropertyChanged
    {
        public WorldObject WorldObject { get; private set; }
        public IPartsContainer PartsContainer { get; private set; }

        private IEnumerable<IHasCustomStorageSize> StorageSizeModifyingParts => PartsContainer.Parts.OfType<IHasCustomStorageSize>();
        public int BaseNumberOfSlots { get; set; } = 0;
        public int BaseMaxWeight { get; set; } = 0;
        public void SetModel(WorldObject worldObject, IPartsContainer partsContainer, int baseNumberOfSlots, int baseMaxWeight)
        {
            WorldObject = worldObject;
            PartsContainer = partsContainer;
            BaseNumberOfSlots = baseNumberOfSlots;
            BaseMaxWeight = baseMaxWeight;
            partsContainer.NewPartInSlotEvent.Add(OnPartPropertyChanged);
            OnModelChanged();
        }
        private void OnPartPropertyChanged(Slot slot)
        {
            OnModelChanged();
        }
        private void OnModelChanged()
        {
            int totalNumberOfAdditionalSlots = StorageSizeModifyingParts.Sum(customStorageSizeComponent => customStorageSizeComponent.StorageSizeModifier.NumberOfAdditionalSlots);
            int totalNumberOfSlots = BaseNumberOfSlots + totalNumberOfAdditionalSlots;
            SetStorageSize(totalNumberOfSlots);
        }
        private void SetStorageSize(int totalNumberOfSlots)
        {
            PublicStorageComponent publicStorageComponent = WorldObject.GetComponent<PublicStorageComponent>();

            LimitedInventory storage = publicStorageComponent.Storage as LimitedInventory;
            if (storage != null)
            {
                List<ItemStack> newStacks = new List<ItemStack>();
                for (int i = 0; i < totalNumberOfSlots; i++)
                {
                    newStacks.Add(new ItemStack());
                }
                storage.ReplaceStacks(newStacks);
                storage.Changed(nameof(Inventory.Stacks));
                publicStorageComponent.Parent.SetDirty();
                Log.WriteLine(Localizer.DoStr("Replacing stacks"));

            }
            Log.WriteLine(Localizer.DoStr("Reset public storage to:" + publicStorageComponent?.Storage.Stacks.Count() + "," + BaseMaxWeight));
            //SelectionStorageComponent selectionStorageComponent = WorldObject.GetComponent<SelectionStorageComponent>();
            //selectionStorageComponent?.CreateInventory(totalNumberOfSlots, BaseMaxWeight);
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(PartSlotsUIComponent))]
    [PriorityAfter(typeof(PublicStorageComponent), typeof(PartsContainerComponent))]
    public class StorageSizeModififierComponent : WorldObjectComponent
    {
        private StorageSizeSetterViewController View { get; set; }
        private IPartsContainer PartsContainer { get; set; }

        [Serialized] private int baseNumSlots;
        [Serialized] private int baseWeightLimit;
        [Serialized] private bool isNewObject = true;
        private WeightComponent WeightComponent { get; set; }
        public override void Initialize()
        {
            base.Initialize();
            PartsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;
            
            Inventory publicStorageInventory = Parent.GetComponent<PublicStorageComponent>().Storage;
            if (isNewObject) baseNumSlots = publicStorageInventory.Stacks.Count();
            IEnumerable<InventoryComponent> components = typeof(Inventory).GetProperty("Components", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(publicStorageInventory) as IEnumerable<InventoryComponent>;
            WeightComponent = components?.OfType<WeightComponent>().FirstOrDefault();
            baseWeightLimit = WeightComponent?.MaxWeight ?? -1;
            isNewObject = false;
            Log.WriteLine(Localizer.DoStr($"Storage modifier:{baseNumSlots}, {baseWeightLimit}, {components?.Count()}"));
            Log.WriteLine(Localizer.DoStr("parts:" + PartsContainer.Parts.Count()));
            Log.WriteLine(Localizer.DoStr("Modifers:" + PartsContainer.Parts.OfType<IHasCustomStorageSize>().Count()));

        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            BuildViews();
        }
        private void BuildViews()
        {
            StorageSizeSetterViewController partView = new StorageSizeSetterViewController();
            View = partView;
            partView.SetModel(Parent, PartsContainer, baseNumSlots, baseWeightLimit);
        }
    }
}

namespace Eco.Mods.TechTree
{
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(StorageSizeModififierComponent))]
    public partial class TruckObject : IPartsContainerWorldObject
    {
        public IPartsContainerSchema GetPartsContainerSchema() => new TruckSchema();
    }
}