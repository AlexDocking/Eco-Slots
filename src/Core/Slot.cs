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
    public class Slot : ISlot, IController, INotifyPropertyChanged
    {
        [Serialized, SyncToView] public string Name { get; set; }
        [Serialized, SyncToView, NewTooltip(Eco.Shared.Items.CacheAs.Disabled)] public Inventory Inventory
        {
            get => inventory; private set => SetInventory(value);
        }
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

        private WorldObjectHandle? WorldObject
        {
            get => worldObject; set
            {
                worldObject = value;
                this.Inventory.SetOwner(value);
            }
        }
        public Slot()
        {
            Inventory defaultInventory = new AuthorizationInventory(1, AuthorizationFlags.AuthedMayAdd | AuthorizationFlags.AuthedMayRemove);
            SetInventory(defaultInventory);
        }

        private void SetInventory(Inventory newInventory)
        {
            this.inventory?.SetOwner(null);
            this.inventory?.OnChanged.Remove(OnInventoryChanged);
            newInventory?.SetOwner(WorldObject);
            this.inventory = newInventory;
            newInventory?.OnChanged.AddAndCall(OnInventoryChanged, null);
        }
        public void Initialize(WorldObject worldObject, IPartsContainer partsContainer)
        {
            PartsContainer = partsContainer;
            this.WorldObject = worldObject;
        }
        public bool SetPart(IPart part)
        {
            this.Inventory.Stacks.First().ReplaceStack(part as Item, 1, true);
            return true;
        }
        private void OnPartPropertyChanged(IPart part, IPartProperty partProperty)
        {
            PartPropertyChangedEvent.Invoke(this, part, partProperty);
        }
        private void OnInventoryChanged(User user)
        {
            IPart newPart = Inventory.Stacks?.FirstOrDefault()?.Item as IPart;
            if (part == newPart) return;

            part?.PartPropertyChangedEvent.Remove(OnPartPropertyChanged);
            part = newPart;
            newPart?.PartPropertyChangedEvent.Add(OnPartPropertyChanged);

            NewPartInSlotEvent.Invoke();
        }

        public Result TryAddPart(IPart part)
        {
            if (Part == null && part is Item partItem)
            {
                Inventory.TryAddItem(partItem);
                return Result.SetSucceed(Part == part);
            }
            return Result.FailedNoMessage;
        }

        #region IController
        private int id;
        private IPart part;
        private WorldObjectHandle? worldObject;
        private Inventory inventory = new AuthorizationInventory(1);

        public ref int ControllerID => ref id;

        public ISlotDefinition GenericDefinition => null;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
