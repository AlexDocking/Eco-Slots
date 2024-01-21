using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    public interface IEnabledOrDisabled
    {
        bool IsEnabled { get; set; }
    }
    public class InventoryCollectionSet : Inventory
    {
        public ISet<Inventory> Inventories { get; } = new HashSet<Inventory>();
        protected override IEnumerable<Inventory> SubInventories => Inventories;
        public override bool IsLeafInventory => false;
    }
    public class RequireEmptyStoragesRestriction : InventoryRestriction, IEnabledOrDisabled
    {
        public bool IsEnabled { get; set; } = true;
        public override LocString Message => Localizer.DoStr("Storage must be empty");
        public InventoryCollectionSet InventorySet { get; set; } = new InventoryCollectionSet();

        public override int MaxAccepted(RestrictionCheckData checkData, Item item, int currentQuantity)
        {
            if (!IsEnabled) return -1;
            if (!InventorySet.IsEmpty) return 0;
            return base.MaxAccepted(checkData, item, currentQuantity);
        }
        public override int MaxPickup(RestrictionCheckData checkData, Item item, int currentQuantity)
        {
            if (!IsEnabled) return -1;
            if (!InventorySet.IsEmpty) return 0;
            return base.MaxPickup(checkData, item, currentQuantity);
        }
    }
    public class EditableSpecificItemTypesRestriction : InventoryRestriction, IEnabledOrDisabled
    {
        public bool IsEnabled { get; set; } = true;

        public ISet<Type> AllowedItemTypes { get; } = new HashSet<Type>();
        public override LocString Message => Localizer.Do($"Slot only accepts {this.AllowedItemTypes.Select(x => x.UILink()).CommaList()}.");
        public override int MaxAccepted(Item item, int currentQuantity)
        {
            if (!IsEnabled) return -1;
            if (!AllowedItemTypes.Any()) return -1;
            return AllowedItemTypes.Contains(item.Type) ? -1 : 0;
        }
    }
    public class NoRemoveRestriction : InventoryRestriction, IEnabledOrDisabled
    {
        public bool IsEnabled { get; set; } = false;
        public override int Priority => 1000;
        public override LocString Message => Localizer.DoStr("Cannot remove from this slot");
        public override int MaxPickup(RestrictionCheckData checkData, Item item, int currentQuantity)
        {
            if (!IsEnabled) return -1;
            return 0;
        }
    }
    public class PerSlotRestrictions : INotifyPropertyChanged
    {
        private bool isLocked = false;

        private NoRemoveRestriction NoRemoveRestriction { get; set; }
        private EditableSpecificItemTypesRestriction EditableSpecificItemTypesRestriction { get; set; }
        private RequireEmptyStoragesRestriction RequireEmptyStoragesRestriction { get; set; }
        public PerSlotRestrictions(Slot slot)
        {
            NoRemoveRestriction = new NoRemoveRestriction();
            EditableSpecificItemTypesRestriction = new EditableSpecificItemTypesRestriction();
            RequireEmptyStoragesRestriction = new RequireEmptyStoragesRestriction();

            slot.Inventory.AddInvRestriction(NoRemoveRestriction);
            slot.Inventory.AddInvRestriction(EditableSpecificItemTypesRestriction);
            slot.Inventory.AddInvRestriction(RequireEmptyStoragesRestriction);
        }
        public bool IsSlotOptional { get => !NoRemoveRestriction.IsEnabled; set => NoRemoveRestriction.IsEnabled = !value; }
        public bool IsLocked => isLocked;

        public IEnumerable<Inventory> GetRequiredEmptyStorages()
        {
            return RequireEmptyStoragesRestriction.InventorySet.Inventories;
        }
        public void AddEmptyRequirementToStorage(Inventory inventory)
        {
            RequireEmptyStoragesRestriction.InventorySet.Inventories.Add(inventory);
            inventory.OnChanged.AddAndCall(OnInventoryChanged, null);
        }
        private void OnInventoryChanged(User user)
        {
            bool currentlyLocked = isLocked;
            isLocked = GetRequiredEmptyStorages().Any(inventory => !inventory.IsEmpty);
            if (isLocked != currentlyLocked) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLocked)));
        }
        public IEnumerable<Type> AllowedItemTypes { get => EditableSpecificItemTypesRestriction.AllowedItemTypes; set { EditableSpecificItemTypesRestriction.AllowedItemTypes.Clear(); EditableSpecificItemTypesRestriction.AllowedItemTypes.AddRange(value); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
