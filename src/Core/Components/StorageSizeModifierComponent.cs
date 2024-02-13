using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Parts
{
    /// <summary>
    /// Changes the number of public storage slots the world object has.
    /// When the object is first placed, this component will record how many slots the storage has.
    /// Since the base number of storage slots is set through <see cref="WorldObject.Initialize"/>,
    /// the storage component does not record what this number is, and we do not want to override every object's initialization in order to tell this component the same number,
    /// we can catch the storage after it is first created and work out how many slots it has by default.
    /// Afterwards we can increase or decrease the number of storage slots, and always know what the base number is.
    /// The functionality of modifying the storage itself done through <see cref="StorageSizeSetter"/>.
    /// TODO: modify the storage weight limit.
    /// TODO: make it work with <see cref="SelectionStorageComponent"/>
    /// </summary>
    [Serialized]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    [PriorityAfter(typeof(PublicStorageComponent), typeof(PartsContainerComponent))]
    public sealed class StorageSizeModifierComponent : WorldObjectComponent
    {
        private IPartsContainer PartsContainer { get; set; }

        [Serialized] private int baseNumSlots;
        [Serialized] private int baseWeightLimit;
        /// <summary>
        /// Only record the number of storage slots once, when the object is first placed.
        /// Thereafter every time it is placed, it will have our modified number of slots, so we need to know which state we are in.
        /// </summary>
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
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            BuildViews();
        }
        private void BuildViews()
        {
            StorageSizeSetter partView = new StorageSizeSetter();
            partView.SetModel(Parent, PartsContainer, baseNumSlots, baseWeightLimit);
        }
    }
}
