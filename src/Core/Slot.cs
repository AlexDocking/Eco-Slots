using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using static Eco.Gameplay.Items.AuthorizationInventory;

namespace Parts
{
    [Serialized]
    public class Slot : IController, INotifyPropertyChanged
    {
        [Serialized, SyncToView] public string Name { get; set; }
        [Serialized, SyncToView, NewTooltip(Eco.Shared.Items.CacheAs.Disabled)] public Inventory Inventory { get; private set; } = new AuthorizationInventory(1);

        [SyncToView]
        public IPart Part => part;

        public IPartsContainer PartsContainer { get; private set; }
        /// <summary>
        /// Called whenever the slot inventory changes
        /// </summary>
        public ThreadSafeAction NewPartInSlotEvent { get; } = new ThreadSafeAction();
        /// <summary>
        /// Called whenever one of the part's properties e.g. colour is changed
        /// </summary>
        public ThreadSafeAction<Slot, IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<Slot, IPart, IPartProperty>();
        public void Initialize(WorldObject worldObject, IPartsContainer partsContainer)
        {
            PartsContainer = partsContainer;
            if (this.Inventory is not AuthorizationInventory)
            {
                // ensure the inventory type is authorization inventory (migration)
                var newInventory = new AuthorizationInventory(
                    this.Inventory.Stacks.Count(),
                    AuthorizationFlags.AuthedMayAdd | AuthorizationFlags.AuthedMayRemove);
                newInventory.ReplaceStacks(this.Inventory.Stacks);
                this.Inventory = newInventory;
            }
            this.Inventory.SetOwner(worldObject);
            this.Inventory.OnChanged.Add(OnInventoryChanged);
            SetPart(Inventory.Stacks?.FirstOrDefault()?.Item as IPart);
        }
        private void SetPart(IPart newPart)
        {
            part?.PartPropertyChangedEvent.Remove(OnPartPropertyChanged);
            part = newPart;
            newPart?.PartPropertyChangedEvent.Add(OnPartPropertyChanged);
        }
        private void OnPartPropertyChanged(IPart part, IPartProperty partProperty)
        {
            PartPropertyChangedEvent.Invoke(this, part, partProperty);
        }
        private void OnInventoryChanged(User user)
        {
            SetPart(Inventory.Stacks?.FirstOrDefault()?.Item as IPart);
            NewPartInSlotEvent.Invoke();
        }

        public void TryAddPart(IPart part)
        {
            if (part is Item partItem)
            {
                Inventory.TryAddItem(partItem);
            }
        }

        #region IController
        private int id;
        private IPart part;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
