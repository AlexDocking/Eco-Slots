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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Eco.Gameplay.Items.AuthorizationInventory;

namespace Parts
{
    [Serialized]
    public class InventorySlot : ISlot, IController, INotifyPropertyChanged
    {
        [Serialized, SyncToView] public string Name { get => GenericDefinition?.Name ?? string.Empty; set { } }
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
        public ThreadSafeAction<ISlot, IPart, IPartProperty> PartPropertyChangedEvent { get; } = new ThreadSafeAction<ISlot, IPart, IPartProperty>();

        private WorldObject WorldObject
        {
            get
            {
                if (worldObjectIsSet)
                {
                    return worldObjectHandle;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                worldObjectHandle = value;
                worldObjectIsSet = true;
                this.Inventory.SetOwner(value);
            }
        }
        public InventorySlot()
        {
            Inventory defaultInventory = new AuthorizationInventory(1, AuthorizationFlags.AuthedMayAdd | AuthorizationFlags.AuthedMayRemove);
            SetInventory(defaultInventory);
        }
        public InventorySlot(ISlotDefinition slotDefinition) : this()
        {
            GenericDefinition = slotDefinition;
            if (!slotDefinition.CanPartEverBeRemoved)
            {
                NoRemoveRestriction restriction = new NoRemoveRestriction() { IsEnabled = true };
                Inventory.AddInvRestriction(restriction);
            }
            if (slotDefinition.CanPartEverBeAdded)
            {
                if (slotDefinition.RestrictionsToAddPart.FirstOrDefault(restrictionToAdd => restrictionToAdd is LimitedTypeSlotRestriction) is LimitedTypeSlotRestriction limitedTypeSlotRestriction)
                {
                    EditableSpecificItemTypesRestriction restriction = new EditableSpecificItemTypesRestriction();
                    restriction.AllowedItemTypes.AddRange(limitedTypeSlotRestriction.AllowedTypes);
                    Inventory.AddInvRestriction(restriction);
                }
            }
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
                return Inventory.TryAddItem(partItem);
            }

            return Result.FailedNoMessage;
        }
        public Result TrySetPart(IPart part)
        {
            if (CanSetPart(part)) { SetPart(part); return Result.Succeeded; }
            return Result.FailedNoMessage;
        }
        public bool CanSetPart(IPart part)
        {
            return CanAcceptPart(part) && (Part == null || CanRemovePart());
        }
        public Result CanAcceptPart(IPart part)
        {
            if (part is not Item partItem) return Result.Fail(Localizer.Do($"{part?.GetType().Name ?? "null"} is not a Item"));
            if (Inventory.AcceptsItem(partItem)) return Result.Succeeded;
            return Result.FailedNoMessage;
        }
        public bool CanRemovePart()
        {
            if (Part is not Item partItem) return false;
            return Inventory.GetMaxPickup(partItem, 1).Val > 0;
        }

        #region IController
        private int id;
        private IPart part;
        private bool worldObjectIsSet = false;
        private WorldObjectHandle worldObjectHandle = null;
        private Inventory inventory = new AuthorizationInventory(1);

        public ref int ControllerID => ref id;

        public virtual ISlotDefinition GenericDefinition { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
