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
            SetName(existingContainer);
            SetOptional(existingContainer, slotRestrictionManager);
            SetAllowedTypes(existingContainer, slotRestrictionManager);
            SetDefaultParts(existingContainer);
            SetEmptyStoragesRestriction(existingContainer, slotRestrictionManager);
            return existingContainer;
        }
        private void EnsureCorrectNumberOfSlots(IPartsContainer partsContainer)
        {
            IReadOnlyList<Slot> slots = partsContainer.Slots;
            for (int i = 0; i < SlotDefinitions.Count - slots.Count; i++)
            {
                partsContainer.AddPart(new Slot(), null);
            }
        }
        private void SetName(IPartsContainer partsContainer)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                Slot slot = partsContainer.Slots[i];
                SlotDefinition slotDefinition = SlotDefinitions[i];
                slot.Name = slotDefinition.Name;
            }
        }
        private void SetOptional(IPartsContainer partsContainer, BasicSlotRestrictionManager slotRestrictionManager)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                Slot slot = partsContainer.Slots[i];
                SlotDefinition slotDefinition = SlotDefinitions[i];
                slotRestrictionManager.SetOptional(slot, slotDefinition.Optional);
            }
        }
        private void SetAllowedTypes(IPartsContainer partsContainer, BasicSlotRestrictionManager slotRestrictionManager)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                Slot slot = partsContainer.Slots[i];
                SlotDefinition slotDefinition = SlotDefinitions[i];
                slotRestrictionManager.SetTypeRestriction(slot, slotDefinition.AllowedItemTypes);
            }
        }
        private void SetDefaultParts(IPartsContainer partsContainer)
        {
            for (int i = 0; i < partsContainer.Slots.Count; i++)
            {
                Slot slot = partsContainer.Slots[i];
                SlotDefinition slotDefinition = SlotDefinitions[i];
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
                Slot slot = partsContainer.Slots[i];
                SlotDefinition slotDefinition = SlotDefinitions[i];
                foreach (Inventory inventory in slotDefinition.StoragesThatMustBeEmpty)
                {
                    restrictionManager.AddRequiredEmptyStorage(slot, inventory);
                }
            }
        }

    }
    public class SlotDefinitions : IList<SlotDefinition>
    {
        public SlotDefinition this[int index] { get => ((IList<SlotDefinition>)List)[index]; set => ((IList<SlotDefinition>)List)[index] = value; }

        public int Count => ((ICollection<SlotDefinition>)List).Count;

        public bool IsReadOnly => ((ICollection<SlotDefinition>)List).IsReadOnly;

        private List<SlotDefinition> List { get; } = new List<SlotDefinition>();

        public void Add(SlotDefinition item)
        {
            ((ICollection<SlotDefinition>)List).Add(item);
        }

        public void Clear()
        {
            ((ICollection<SlotDefinition>)List).Clear();
        }

        public bool Contains(SlotDefinition item)
        {
            return ((ICollection<SlotDefinition>)List).Contains(item);
        }

        public void CopyTo(SlotDefinition[] array, int arrayIndex)
        {
            ((ICollection<SlotDefinition>)List).CopyTo(array, arrayIndex);
        }

        public IEnumerator<SlotDefinition> GetEnumerator()
        {
            return ((IEnumerable<SlotDefinition>)List).GetEnumerator();
        }

        public int IndexOf(SlotDefinition item)
        {
            return ((IList<SlotDefinition>)List).IndexOf(item);
        }

        public void Insert(int index, SlotDefinition item)
        {
            ((IList<SlotDefinition>)List).Insert(index, item);
        }

        public bool Remove(SlotDefinition item)
        {
            return ((ICollection<SlotDefinition>)List).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<SlotDefinition>)List).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)List).GetEnumerator();
        }
    }
    public class SlotDefinition : RegularInventorySlotDefinition
    {
        public Func<IPart> MustHavePart { get; set; }
        public IEnumerable<Inventory> StoragesThatMustBeEmpty { get; set; } = Enumerable.Empty<Inventory>();
        public Func<IPart> MustHavePartIfEmpty { get; set; }
    }
}
