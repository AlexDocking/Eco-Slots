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
    /// <summary>
    /// A collection of inventories, used for determining whether the world object's storages are empty or not.
    /// </summary>
    public class InventoryCollectionSet : Inventory
    {
        public ISet<Inventory> Inventories { get; } = new HashSet<Inventory>();
        protected override IEnumerable<Inventory> SubInventories => Inventories;
        public override bool IsLeafInventory => false;
    }
    public class RequireEmptyStorageToAddRestriction : InventoryRestriction
    {
        public override LocString Message => Localizer.DoStr("Storage must be empty");
        public InventoryCollectionSet InventorySet { get; set; } = new InventoryCollectionSet();

        public override int MaxAccepted(Item item, int currentQuantity)
        {
            if (!InventorySet.IsEmpty) return 0;
            return base.MaxAccepted(item, currentQuantity);
        }
    }
    public class RequireEmptyStorageToRemoveRestriction : InventoryRestriction
    {
        public override LocString Message => Localizer.DoStr("Storage must be empty");
        public InventoryCollectionSet InventorySet { get; set; } = new InventoryCollectionSet();
        public override int MaxPickup(RestrictionCheckData checkData, Item item, int currentQuantity)
        {
            if (!InventorySet.IsEmpty) return 0;
            return base.MaxPickup(checkData, item, currentQuantity);
        }
    }
    public class EditableSpecificItemTypesRestriction : InventoryRestriction
    {
        public override RestrictionType Type => RestrictionType.Specialized;
        public ISet<Type> AllowedItemTypes { get; } = new HashSet<Type>();
        public override LocString Message => Localizer.Do($"Slot only accepts {this.AllowedItemTypes.Select(x => x.UILink()).CommaList()}.");
        public override int MaxAccepted(Item item, int currentQuantity)
        {
            if (!AllowedItemTypes.Any()) return -1;
            return AllowedItemTypes.Contains(item.Type) ? -1 : 0;
        }
    }
    public class NoRemoveRestriction : InventoryRestriction
    {
        public override int Priority => 1000;
        public override LocString Message => Localizer.DoStr("Cannot remove from this slot");
        public override int MaxPickup(RestrictionCheckData checkData, Item item, int currentQuantity)
        {
            return 0;
        }
    }
}
