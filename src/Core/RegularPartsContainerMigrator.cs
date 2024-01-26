﻿using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Localization.ConstLocs;
using Eco.Shared.Utils;
using Eco.Simulation.WorldLayers.Pullers;
using Parts.Migration;
using Parts.WIP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Parts
{
    public class RegularPartsContainerMigrator : IPartsContainerMigrator
    {
        public SlotDefinitions SlotDefinitions { get; set; }

        public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
        {
            IPartsContainer newContainer = PartsContainerFactory.Create();
            EnsureCorrectNumberOfSlots(existingContainer, newContainer);
            SetDefaultParts(newContainer);
            return newContainer;
        }
        private void EnsureCorrectNumberOfSlots(IPartsContainer existingPartsContainer, IPartsContainer newPartsContainer)
        {
            IReadOnlyList<ISlot> slots = existingPartsContainer.Slots;
            for (int i = 0; i < SlotDefinitions.Count; i++)
            {
                IPart existingPart = i < slots.Count ? slots[i].Part : null;
                newPartsContainer.TryAddSlot(new InventorySlot(SlotDefinitions[i]), existingPart);
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
        public Func<IPart> MustHavePartIfEmpty { get; set; }
        public IEnumerable<Type> AllowedItemTypes { get; init; } = new HashSet<Type>();
        public bool RequiresEmptyStorageToChangePart { get; set; } = false;
        public bool CanPartEverBeAdded => AllowedItemTypes?.Any() ?? false;
        public bool CanPartEverBeRemoved => Optional;
        public IEnumerable<ISlotAddRestriction> RestrictionsToAddPart
        {
            get
            {
                List<ISlotAddRestriction> restrictions = new List<ISlotAddRestriction>();
                if (AllowedItemTypes?.Any() ?? false)
                {
                    restrictions.Add(new LimitedTypeSlotRestriction(AllowedItemTypes));
                }
                if (RequiresEmptyStorageToChangePart)
                {
                    restrictions.Add(new RequiresEmptyPublicStorageToAddSlotRestriction());
                }
                return restrictions;
            }
        }
        public IEnumerable<ISlotRemoveRestriction> RestrictionsToRemovePart
        {
            get
            {
                List<ISlotRemoveRestriction> restrictions = new List<ISlotRemoveRestriction>();
                if (RequiresEmptyStorageToChangePart)
                {
                    restrictions.Add(new RequiresEmptyPublicStorageToRemoveSlotRestriction());
                }
                return restrictions;
            }
        }

        public LocString Tooltip()
        {
            LocStringBuilder tooltipBuilder = new LocStringBuilder();
            var restrictions = RestrictionsToAddPart.ToList();
            if (RestrictionsToAddPart.FirstOrDefault(restriction => restriction is LimitedTypeSlotRestriction) is LimitedTypeSlotRestriction limitedTypeSlotRestriction)
            {
                tooltipBuilder.AppendLine(Localizer.DoStr("Can be") + " " + limitedTypeSlotRestriction.AllowedTypes.Select(type => type.UILink()).CommaList(CommonLocs.None, CommonLocs.Or));
                restrictions.Remove(limitedTypeSlotRestriction);
            }
            else
            {
            }
            IEnumerable<LocString> restrictionDescriptions = restrictions.Select(restriction => restriction.Describe());
            if (restrictionDescriptions.Any())
            {
                tooltipBuilder.AppendLine(Localizer.DoStr("Requirements:"));
                tooltipBuilder.AppendLine(restrictionDescriptions.TextList($"\n{CommonLocs.And}\n"));
            }

            return tooltipBuilder.ToLocString();
        }
    }
}