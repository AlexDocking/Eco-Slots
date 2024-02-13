using Eco.Core.Controller;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    public class StorageSizeSetter : IController, INotifyPropertyChanged
    {
        public WorldObject WorldObject { get; private set; }
        public IPartsContainer PartsContainer { get; private set; }

        private IEnumerable<IPartWithCustomStorageSize> StorageSizeModifyingParts => PartsContainer.Parts.OfType<IPartWithCustomStorageSize>();
        public int BaseNumberOfSlots { get; set; } = 0;
        //TODO: add a part property for extra weight allowance and update the weight limit.
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
        private void OnPartPropertyChanged(ISlot slot)
        {
            OnModelChanged();
        }
        private void OnModelChanged()
        {
            int totalNumberOfAdditionalSlots = StorageSizeModifyingParts.Sum(customStorageSizeComponent => customStorageSizeComponent.StorageSizeModifier.NumberOfAdditionalSlots);
            int totalNumberOfSlots = BaseNumberOfSlots + totalNumberOfAdditionalSlots;
            SetStorageSize(totalNumberOfSlots);
        }
        /// <summary>
        /// Set the number of stacks the public storage has.
        /// TODO: Change the number of stacks on SelectionStorageComponent
        /// TODO: Do something with non-empty stacks. Currently the check for that is done before we get here, but that's no guarantee for the future. As it stands all stacks are replaced in their entirety with new ones.
        /// </summary>
        /// <param name="totalNumberOfSlots"></param>
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
