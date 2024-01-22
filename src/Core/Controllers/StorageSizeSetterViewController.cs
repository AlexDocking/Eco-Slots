using Eco.Core.Controller;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
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
            if (storage != null && storage.Stacks.Count() != totalNumberOfSlots)
            {
                List<ItemStack> newStacks = new List<ItemStack>();
                for (int i = 0; i < totalNumberOfSlots; i++)
                {
                    newStacks.Add(new ItemStack());
                }
                storage.ReplaceStacks(newStacks);
                storage.Changed(nameof(Inventory.Stacks));
                publicStorageComponent.Parent.SetDirty();
            }
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
