using Eco.Core.Controller;
using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    public interface ISlotRestrictionManager
    {
        public LocString DisplayRestriction(Slot slot);
        public bool IsOptional(Slot slot);
        bool IsSlotLocked(Slot slot);
        public ThreadSafeAction<Slot> SlotLockedChangedEvent { get; }
    }

    public class BasicSlotRestrictionManager : ISlotRestrictionManager
    {
        public IPartsContainer PartsContainer { get; private set; }
        public ThreadSafeAction<Slot> SlotLockedChangedEvent { get; } = new ThreadSafeAction<Slot>();
        private IDictionary<Slot, PerSlotRestrictions> SlotRestrictions { get; } = new ThreadSafeDictionary<Slot, PerSlotRestrictions>();
        private PerSlotRestrictions GetOrAddRestrictionsToSlot(Slot slot)
        {
            PerSlotRestrictions perSlotRestrictions;
            if (!SlotRestrictions.TryGetValue(slot, out perSlotRestrictions))
            {
                perSlotRestrictions = new PerSlotRestrictions(slot);
                if (SlotRestrictions.TryAdd(slot, perSlotRestrictions))
                {
                    perSlotRestrictions.PropertyChanged += (object _, PropertyChangedEventArgs args) =>
                    {
                        if (args.PropertyName == nameof(PerSlotRestrictions.IsLocked)) SlotLockedChangedEvent.Invoke(slot);
                    };
                }
            }
            perSlotRestrictions = SlotRestrictions[slot];
            return perSlotRestrictions;
        }
        public void SetOptional(Slot slot, bool isOptional)
        {
            PerSlotRestrictions slotRestrictions = GetOrAddRestrictionsToSlot(slot);
            slotRestrictions.IsSlotOptional = isOptional;
        }
        public void AddRequiredEmptyStorage(Slot slot, Inventory mustBeEmpty) => GetOrAddRestrictionsToSlot(slot).AddEmptyRequirementToStorage(mustBeEmpty);
        public void SetTypeRestriction(Slot slot, IEnumerable<Type> validItemTypes) => GetOrAddRestrictionsToSlot(slot).AllowedItemTypes = validItemTypes;
        private IEnumerable<Type> AllowedItemTypes(Slot slot) => GetOrAddRestrictionsToSlot(slot).AllowedItemTypes;
        public LocString DisplayRestriction(Slot slot)
        {
            IEnumerable<Item> allowedItems = AllowedItemTypes(slot).Select(type => Item.Get(type)).NonNull();
            return Localizer.DoStr("Accepts").AppendSpaceIfSet() + allowedItems.Select(item => item.UILink()).CommaList();
        }
        public bool IsSlotLocked(Slot slot) => GetOrAddRestrictionsToSlot(slot).IsLocked;
        public bool IsOptional(Slot slot) => GetOrAddRestrictionsToSlot(slot).IsSlotOptional;
    }
}
