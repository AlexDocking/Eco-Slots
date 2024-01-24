using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Parts.Migration;
using Parts.Tests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    public class RegularSchema : IPartsContainerSchema
    {
        public SlotDefinitions SlotDefinitions { get; set; }

        public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
        {
            BasicSlotRestrictionManager slotRestrictionManager = new BasicSlotRestrictionManager();
            existingContainer.SlotRestrictionManager = slotRestrictionManager;
            EnsureCorrectNumberOfSlots(existingContainer);
            SetOptional(existingContainer, slotRestrictionManager);
            SetAllowedTypes(existingContainer, slotRestrictionManager);
            SetDefaultParts(existingContainer);
            SetEmptyStoragesRestriction(existingContainer, slotRestrictionManager);
            return existingContainer;
        }
        private void EnsureCorrectNumberOfSlots(IPartsContainer partsContainer)
        {
            IReadOnlyList<ISlot> slots = partsContainer.Slots;
            for (int i = slots.Count; i < SlotDefinitions.Count; i++)
            {
                partsContainer.TryAddSlot(new InventorySlot(SlotDefinitions[i]), null);
            }
        }
        private void SetOptional(IPartsContainer partsContainer, BasicSlotRestrictionManager slotRestrictionManager)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                ISlot slot = partsContainer.Slots[i];
                RegularSlotDefinition slotDefinition = SlotDefinitions[i];
                slotRestrictionManager.SetOptional(slot, slotDefinition.Optional);
            }
        }
        private void SetAllowedTypes(IPartsContainer partsContainer, BasicSlotRestrictionManager slotRestrictionManager)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                ISlot slot = partsContainer.Slots[i];
                RegularSlotDefinition slotDefinition = SlotDefinitions[i];
                slotRestrictionManager.SetTypeRestriction(slot, slotDefinition.AllowedItemTypes);
            }
        }
        private void SetDefaultParts(IPartsContainer partsContainer)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                ISlot slot = partsContainer.Slots[i];
                RegularSlotDefinition slotDefinition = SlotDefinitions[i];
                if (slotDefinition.MustHavePart != null) slot.SetPart(slotDefinition.MustHavePart());
                else if (slot.Part == null && slotDefinition.MustHavePartIfEmpty != null)
                {
                    slot.SetPart(slotDefinition.MustHavePartIfEmpty());
                }
            }
        }
        private void SetEmptyStoragesRestriction(IPartsContainer partsContainer, BasicSlotRestrictionManager restrictionManager)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                ISlot slot = partsContainer.Slots[i];
                RegularSlotDefinition slotDefinition = SlotDefinitions[i];
                foreach (Inventory inventory in slotDefinition.StoragesThatMustBeEmpty)
                {
                    restrictionManager.AddRequiredEmptyStorage(slot, inventory);
                }
            }
        }
    }
    public class SlotDefinitions : IList<RegularSlotDefinition>
    {
        public RegularSlotDefinition this[int index] { get => ((IList<RegularSlotDefinition>)List)[index]; set => ((IList<RegularSlotDefinition>)List)[index] = value; }

        public int Count => ((ICollection<RegularSlotDefinition>)List).Count;

        public bool IsReadOnly => ((ICollection<RegularSlotDefinition>)List).IsReadOnly;

        private List<RegularSlotDefinition> List { get; } = new List<RegularSlotDefinition>();

        public void Add(RegularSlotDefinition item)
        {
            ((ICollection<RegularSlotDefinition>)List).Add(item);
        }

        public void Clear()
        {
            ((ICollection<RegularSlotDefinition>)List).Clear();
        }

        public bool Contains(RegularSlotDefinition item)
        {
            return ((ICollection<RegularSlotDefinition>)List).Contains(item);
        }

        public void CopyTo(RegularSlotDefinition[] array, int arrayIndex)
        {
            ((ICollection<RegularSlotDefinition>)List).CopyTo(array, arrayIndex);
        }

        public IEnumerator<RegularSlotDefinition> GetEnumerator()
        {
            return ((IEnumerable<RegularSlotDefinition>)List).GetEnumerator();
        }

        public int IndexOf(RegularSlotDefinition item)
        {
            return ((IList<RegularSlotDefinition>)List).IndexOf(item);
        }

        public void Insert(int index, RegularSlotDefinition item)
        {
            ((IList<RegularSlotDefinition>)List).Insert(index, item);
        }

        public bool Remove(RegularSlotDefinition item)
        {
            return ((ICollection<RegularSlotDefinition>)List).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<RegularSlotDefinition>)List).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)List).GetEnumerator();
        }
    }
    public class RegularSlotDefinition : ISlotDefinition, IOptionalSlotDefinition, ILimitedTypesSlotDefinition, IRequireEmptyStorageSlotDefinition
    {
        public string Name { get; init; }
        public bool Optional { get; init; }
        public Func<IPart> MustHavePart { get; set; }
        public IEnumerable<Inventory> StoragesThatMustBeEmpty { get; set; } = Enumerable.Empty<Inventory>();
        public Func<IPart> MustHavePartIfEmpty { get; set; }
        public IEnumerable<Type> AllowedItemTypes { get; init; } = new HashSet<Type>();
        public bool RequiresEmptyStorageToChangePart => StoragesThatMustBeEmpty?.Any() ?? false;
    }
}
