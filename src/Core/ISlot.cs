﻿using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    [Serialized]
    public interface ISlot
    {
        string Name { get; }

        IPart Part { get; }
        ISlotDefinition GenericDefinition { get; }
        IPartsContainer PartsContainer { get; }
        ThreadSafeAction NewPartInSlotEvent { get; }
        ThreadSafeAction<ISlot, IPart, IPartProperty> PartPropertyChangedEvent { get; }
        ThreadSafeAction<ISlot> AddableOrRemovableChangedEvent { get; }
        Result CanAcceptPart(IPart validPart);
        Result CanRemovePart();
        Result CanSetPart(IPart part);
        void Initialize(WorldObject worldObject, IPartsContainer partsContainer);
        bool SetPart(IPart part);
        Result TryAddPart(IPart part);
        Result TrySetPart(IPart part);
    }
    public interface ISlot<T> : ISlot where T : ISlotDefinition
    {
        T Definition { get; }
    }
    public interface ISlotDefinition
    {
        string Name { get; }
        bool CanPartEverBeAdded { get; }
        bool CanPartEverBeRemoved { get; }
        IEnumerable<ISlotAddRestriction> RestrictionsToAddPart { get; }
        IEnumerable<ISlotRemoveRestriction> RestrictionsToRemovePart { get; }

        ISlot MakeSlotFromDefinition();
        LocString Tooltip();
    }
    public interface ISlotAddRestriction
    {
        LocString Describe();
    }
    public interface ISlotRemoveRestriction
    {

    }
    public class LimitedTypeSlotRestriction : ISlotAddRestriction
    {
        public IEnumerable<Type> AllowedTypes { get; } = new List<Type>();
        public LimitedTypeSlotRestriction(IEnumerable<Type> allowedTypes)
        {
            AllowedTypes = allowedTypes.ToList();
        }

        public LocString Describe() => Localizer.Do($"Part is {AllowedTypes.Select(type => type.UILink()).CommaList()}");
    }
    public class RequiresEmptyPublicStorageToAddSlotRestriction : ISlotAddRestriction
    {
        public LocString Describe() => Localizer.DoStr("Storage is empty");
    }
    public class RequiresEmptyPublicStorageToRemoveSlotRestriction : ISlotRemoveRestriction
    {
        public LocString Describe() => Localizer.DoStr("Storage is empty");
    }
    public interface IOptionalSlotDefinition : ISlotDefinition
    {
        bool Optional { get; }
    }
    public interface ILimitedTypesSlotDefinition : ISlotDefinition
    {
        IEnumerable<Type> AllowedItemTypes { get; }
    }
    public interface IRequireEmptyStorageSlotDefinition : ISlotDefinition
    {
        bool RequiresEmptyStorageToChangePart { get; }
    }
}
