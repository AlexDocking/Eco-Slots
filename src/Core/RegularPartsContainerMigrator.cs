using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Localization.ConstLocs;
using Eco.Shared.Utils;
using Parts.Migration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    /// <summary>
    /// The default way to define the slots for an object, and has support for common scenarios.
    /// It also is responsible for migrating any previously serialized container if the schema changes.
    /// TODO: separate the schema from the migrator.
    /// </summary>
    public class DefaultPartsContainerMigrator : IPartsContainerMigrator
    {
        public SlotDefinitions SlotDefinitions { get; set; }

        public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
        {
            IPartsContainer newContainer = PartsContainerFactory.Create(new PartsContainerSchema(SlotDefinitions));
            SetParts(newContainer, existingContainer);
            return newContainer;
        }
        private void SetParts(IPartsContainer newPartsContainer, IPartsContainer existingPartsContainer)
        {
            for (int i = 0; i < newPartsContainer.Slots.Count; i++)
            {
                ISlot slot = newPartsContainer.Slots[i];
                DefaultInventorySlotDefinition slotDefinition = SlotDefinitions[i];
                if (existingPartsContainer.Slots.Count > i)
                {
                    slot.SetPart(existingPartsContainer.Slots[i].Part);
                }
                if (slotDefinition.MustHavePart != null) slot.SetPart(slotDefinition.MustHavePart());
                else if (slot.Part == null && slotDefinition.MustHavePartIfEmpty != null)
                {
                    slot.SetPart(slotDefinition.MustHavePartIfEmpty());
                }
            }
        }
    }
    public class SlotDefinitions : IList<DefaultInventorySlotDefinition>
    {
        public DefaultInventorySlotDefinition this[int index] { get => ((IList<DefaultInventorySlotDefinition>)List)[index]; set => ((IList<DefaultInventorySlotDefinition>)List)[index] = value; }

        public int Count => ((ICollection<DefaultInventorySlotDefinition>)List).Count;

        public bool IsReadOnly => ((ICollection<DefaultInventorySlotDefinition>)List).IsReadOnly;

        private List<DefaultInventorySlotDefinition> List { get; } = new List<DefaultInventorySlotDefinition>();

        public void Add(DefaultInventorySlotDefinition item)
        {
            ((ICollection<DefaultInventorySlotDefinition>)List).Add(item);
        }

        public void Clear()
        {
            ((ICollection<DefaultInventorySlotDefinition>)List).Clear();
        }

        public bool Contains(DefaultInventorySlotDefinition item)
        {
            return ((ICollection<DefaultInventorySlotDefinition>)List).Contains(item);
        }

        public void CopyTo(DefaultInventorySlotDefinition[] array, int arrayIndex)
        {
            ((ICollection<DefaultInventorySlotDefinition>)List).CopyTo(array, arrayIndex);
        }

        public IEnumerator<DefaultInventorySlotDefinition> GetEnumerator()
        {
            return ((IEnumerable<DefaultInventorySlotDefinition>)List).GetEnumerator();
        }

        public int IndexOf(DefaultInventorySlotDefinition item)
        {
            return ((IList<DefaultInventorySlotDefinition>)List).IndexOf(item);
        }

        public void Insert(int index, DefaultInventorySlotDefinition item)
        {
            ((IList<DefaultInventorySlotDefinition>)List).Insert(index, item);
        }

        public bool Remove(DefaultInventorySlotDefinition item)
        {
            return ((ICollection<DefaultInventorySlotDefinition>)List).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<DefaultInventorySlotDefinition>)List).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)List).GetEnumerator();
        }
    }
    public class DefaultInventorySlotDefinition : ISlotDefinition, IOptionalSlotDefinition, ILimitedTypesSlotDefinition, IRequireEmptyStorageSlotDefinition
    {
        public string Name { get; init; }
        public bool Optional { get; init; }
        /// <summary>
        /// Ensure that the slot has this part.
        /// </summary>
        public Func<IPart> MustHavePart { get; set; }
        /// <summary>
        /// If the slot is empty, either because it is a new object, or it is a new requirement since the last start up, populate the slot with this part.
        /// </summary>
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

        public LocString TooltipContent()
        {
            LocStringBuilder tooltipBuilder = new LocStringBuilder();
            var restrictions = RestrictionsToAddPart.ToList();
            if (restrictions.FirstOrDefault(restriction => restriction is LimitedTypeSlotRestriction) is LimitedTypeSlotRestriction limitedTypeSlotRestriction)
            {
                if (Optional) tooltipBuilder.AppendLine(Localizer.DoStr("Can be") + " " + limitedTypeSlotRestriction.AllowedTypes.Select(type => type.UILink()).CommaList(CommonLocs.None, CommonLocs.Or));
                else tooltipBuilder.AppendLine(Localizer.DoStr("Must be") + " " + limitedTypeSlotRestriction.AllowedTypes.Select(type => type.UILink()).CommaList(CommonLocs.None, CommonLocs.Or));

                restrictions.Remove(limitedTypeSlotRestriction);
            }

            return tooltipBuilder.ToLocString();
        }
        public LocString TooltipTitle()
        {
            LocString title = Localizer.Do($"Slot: {Name}");
            if (Optional) title += Localizer.DoStr(" [Optional]");
            title = title.Style(Text.Styles.Header);
            return title;
        }
        public ISlot MakeSlotFromDefinition()
        {
            return new InventorySlot(this);
        }
    }
}
