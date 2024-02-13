using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    /// <summary>
    /// Restrictions on what and when a slot can accept or not accept a part.
    /// This is only a specification that this restriction should exist, and it's up to the slot to ensure it is complied with.
    /// </summary>
    public interface ISlotRestriction
    {
        /// <summary>
        /// Describe the restriction if it were valid.
        /// </summary>
        LocString Describe();
    }
    public interface ISlotAddRestriction : ISlotRestriction
    {
    }
    public interface ISlotRemoveRestriction : ISlotRestriction
    {
    }
    /// <summary>
    /// List of part types which the slot should allow.
    /// </summary>
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
