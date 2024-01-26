using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Serialization;
using Parts;
using Parts.Migration;
using Parts.Vehicles;
using System;

namespace Parts.Vehicles
{
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
}

namespace Eco.Mods.TechTree
{
    [RequireComponent(typeof(PartsContainerComponent))]
    [RequireComponent(typeof(StorageSizeModifierComponent))]
    [RequireComponent(typeof(PartSlotsUIComponent))]
    public partial class TruckObject : IPartsContainerWorldObject
    {
        public IPartsContainerMigrator GetPartsContainerMigrator() => new TruckMigrator(this);
    }
}