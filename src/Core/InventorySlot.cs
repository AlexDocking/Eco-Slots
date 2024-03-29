﻿using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.TextLinks;
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
    /// <summary>
    /// A parts slot that has an inventory with one stack that holds at most one item.
    /// TODO: ensure it cannot stack above 1.
    /// </summary>
    [Serialized]
    public class InventorySlot : ISlot, IController, INotifyPropertyChanged
    {
        [Serialized, SyncToView] public string Name { get => SlotDefinition?.Name ?? string.Empty; set { } }
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
        /// <summary>
        /// Called whenever the accepted range of parts to add or remove changes
        /// </summary>
        public ThreadSafeAction<ISlot> SlotStatusChanged { get; } = new ThreadSafeAction<ISlot>();
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
                if (value.TryGetComponent(out PublicStorageComponent component))
                {
                    Inventory storage = component.Storage;
                    bool trackEmptyStorage = SlotDefinition.RestrictionsToAddPart.Any(restrictionToAdd => restrictionToAdd is RequiresEmptyPublicStorageToAddSlotRestriction) || SlotDefinition.RestrictionsToRemovePart.Any(restrictionToRemove => restrictionToRemove is RequiresEmptyPublicStorageToRemoveSlotRestriction);
                    if (storage != null && trackEmptyStorage)
                    {
                        RequireEmptyStorageSlotStatus = new RequireEmptyStorageSlotStatus(storage);
                    }
                }
                if (WorldObject != null) SlotRestrictionManager.CreateInventoryRestrictionsBasedOnWorldObject(WorldObject);
            }
        }
        private void OnSlotStatusChanged()
        {
            SlotStatusChanged.Invoke(this);
        }
        /// <summary>
        /// If it is required that the storage be empty in order to add or remove the part, this tracks its status.
        /// TODO: Refactor. What if a slot wanted something different such as only allowing parts to added when not empty but not allow removal?
        /// </summary>
        public RequireEmptyStorageSlotStatus RequireEmptyStorageSlotStatus
        {
            get => requireEmptyStorageSlotStatus; set
            {
                requireEmptyStorageSlotStatus = value;
                RequireEmptyStorageSlotStatus.StatusChangedEvent.Add(OnSlotStatusChanged);
            }
        }

        private InventorySlot()
        {
            Inventory defaultInventory = new AuthorizationInventory(1, AuthorizationFlags.AuthedMayAdd | AuthorizationFlags.AuthedMayRemove);
            SlotRestrictionManager = new InventorySlotRestrictionManager(this); 
            SetInventory(defaultInventory);
            SlotDefinition = new DefaultInventorySlotDefinition();
        }
        public InventorySlot(ISlotDefinition slotDefinition) : this()
        {
            SlotDefinition = slotDefinition;
        }

        private void SetInventory(Inventory newInventory)
        {
            this.inventory?.SetOwner(null);
            this.inventory?.OnChanged.Remove(OnInventoryChanged);
            newInventory?.SetOwner(WorldObject);
            this.inventory = newInventory;
            if (WorldObject != null) SlotRestrictionManager.CreateInventoryRestrictionsBasedOnWorldObject(WorldObject);
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

        private InventorySlotRestrictionManager SlotRestrictionManager { get; set; }
        public Result TryAddPart(IPart part)
        {
            if (Part != null) return Result.Fail(Localizer.DoStr("Slot already contains a part"));
            return TrySetPart(part);
        }
        public Result TrySetPart(IPart part)
        {
            if (!SlotRestrictionManager.CanAcceptPart(part, out List<LocString> failureReasons))
            {
                return Result.Fail(failureReasons.NewlineList());
            }
            SetPart(part);
            return Result.Succeeded;
        }
        public Result CanSetPart(IPart part)
        {
            if (!SlotRestrictionManager.CanSetPart(part, out var failureReasons)) return Result.Fail(failureReasons.NewlineList());
            return Result.Succeeded;
        }
        public Result CanAcceptPart(IPart part)
        {
            if (!SlotRestrictionManager.CanAcceptPart(part, out var failureReasons)) return Result.Fail(failureReasons.NewlineList());
            return Result.Succeeded;
        }
        public Result CanAcceptAnyPart() => SlotRestrictionManager.CanAcceptAnyPart();
        public Result CanRemovePart()
        {
            if (!SlotRestrictionManager.CanRemovePart(part, out List<LocString> failureReasons)) return Result.Fail(failureReasons.NewlineList());
            return Result.Succeeded;
        }

        #region IController
        private int id;
        public ref int ControllerID => ref id;

        private IPart part;
        private bool worldObjectIsSet = false;
        private WorldObjectHandle worldObjectHandle = null;
        private Inventory inventory = new AuthorizationInventory(1);
        private RequireEmptyStorageSlotStatus requireEmptyStorageSlotStatus;
        public virtual ISlotDefinition SlotDefinition { get; }

        public LocString Tooltip()
        {
            IPart part = Part;
            LocStringBuilder tooltipBuilder = new LocStringBuilder();
            tooltipBuilder.AppendLine(SlotDefinition.TooltipTitle());
            if (part == null)
            {
                tooltipBuilder.Append(SlotDefinition.TooltipContent());
                return tooltipBuilder.ToLocString();
            }

            Item partItem = part as Item;
            
            tooltipBuilder.Append(Localizer.DoStr("Contains: "));
            LocString partDisplayName = Localizer.DoStr(part.DisplayName).Style(Text.Styles.Name);
            if (partItem != null)
            {
                tooltipBuilder.AppendLine(partItem.UILink(Text.Icon(partItem?.IconName) + partDisplayName));
            }
            else
            {
                tooltipBuilder.AppendLine(partDisplayName);
            }
            tooltipBuilder.AppendLine(ModTooltipLibrary.PartTooltip(part));

            return tooltipBuilder.ToLocString();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
