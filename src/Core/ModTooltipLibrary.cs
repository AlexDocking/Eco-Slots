using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.IoC;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Controller;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;

namespace Parts
{
    /// <summary>
    /// You can add your own tooltip methods to <see cref="TooltipsByType"></see> for your new part property types.
    /// </summary>
    [TooltipLibrary]
    public static class ModTooltipLibrary
    {
        /// <summary>
        /// Tooltip part generators by part property type.
        /// You can make a description of a part's property that will appear in the object's tooltip section of installed parts,
        /// so long as the slot the part is in does not choose to present itself differently.
        /// For a part property for colour you can add a function along these lines:
        /// <code>
        /// <see cref="TooltipsByType"/>.Add(typeof(IColouredPart), (IColouredPart part) => Localizer.DoStr("Colour: ") + part.ColourData.Colour.HexCode); 
        /// </code>
        /// For an InventorySlot called 'Door' that contains a part 'Flat Door', the tooltip when you hover over the World Object instance's name will present like this:
        /// <code>
        /// <b>Slot: Door</b><br></br>
        /// Contains: <b>Flat Door</b><br></br>
        /// Colour: #123456     &lt;-- The slot tooltip asks for a description of the parts' properties, which returns this line. This is the line you just added
        /// </code>
        /// </summary>
        public static ThreadSafeDictionary<Type, Func<object, LocString>> TooltipsByType { get; } = new ThreadSafeDictionary<Type, Func<object, LocString>>();
        public static void Initialize()
        {
            TooltipsByType.Add(typeof(ModelPartColourData), o => ((ModelPartColourData)o).ColourDataTooltip());
            TooltipsByType.Add(typeof(ICustomStorageSize), o => ((ICustomStorageSize)o).CustomStorageSizeTooltip());

            PartNotifications.PartPropertyChangedEventGlobal.Add((part, property) => { if (part is IController controller) ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(SpecificHasModelPartColourTooltip), null, controller); });

            ModelPartColourData.OnColourChangedGlobal.Add(colouring => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(ModelPartColouringTooltip), null, colouring));
            PartsContainer.PartsContainerChangedEventGlobal.Add(partsContainer
                => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(CurrentPartsListDescription), null, partsContainer));
            PartsContainer.PartsContainerChangedEventGlobal.Add(partsContainer
                => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(GenericPartsContainerWorldObjectItemTooltip), null, partsContainer)); 
        }
        /// <summary>
        /// Generates the tooltip for an item's colour data, if that item is not the generic item.
        /// </summary>
        [TooltipAffectedBy(typeof(IColouredPart), nameof(IColouredPart.ColourData), nameof(ModelPartColourData.Colour))]
        [NewTooltip(CacheAs.Instance, 20, overrideType: typeof(IColouredPart))]
        public static LocString SpecificHasModelPartColourTooltip(this IColouredPart part) => part != Item.Get(part.GetType()) ? new TooltipSection(part.ColourData.ModelPartColouringTooltip()) : LocString.Empty;

        /// <summary>
        /// Generates tooltip on items which derive IHasCustomStorageSize
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(IPartWithCustomStorageSize))]
        public static LocString GenericCustomStorageSizeComponentTooltip(Type partType)
        {
            if (Item.Get(partType) is not IPartWithCustomStorageSize part) return LocString.Empty;
            return new TooltipSection(Localizer.DoStr("Storage Size"), part.StorageSizeModifier.CustomStorageSizeTooltip());
        }

        /// <summary>
        /// Tooltip part shown by a slot to show the colour info for its installed part, if that part has a custom storage size.
        /// </summary>
        public static LocString CustomStorageSizeTooltip(this ICustomStorageSize customStorageSize)
        {
            return Localizer.Do($"Increases the number of storage slots by {Text.Info(customStorageSize.NumberOfAdditionalSlots)}").Style(Text.Styles.Info);
        }

        [NewTooltip(CacheAs.Instance, 180, overrideType: typeof(IPartsContainerWorldObjectItem))]
        public static LocString GenericPartsContainerWorldObjectItemTooltip(this IPartsContainerWorldObjectItem partsContainerItem)
        {
            if (partsContainerItem != Item.Get(partsContainerItem.GetType()))
            {
                ItemPersistentData persistentData = (partsContainerItem as IPersistentData)?.PersistentData as ItemPersistentData;
                if (persistentData != null && persistentData.TryGetPersistentData<PartsContainerComponent>(out object partsContainerPersistentData))
                {
                    return LocString.Empty;
                }
            }
            //TODO: separate the container schema from the migrator
            if (partsContainerItem.GetPartsContainerMigrator() is not DefaultPartsContainerMigrator migrator) return LocString.Empty;
            IPartsContainerSchema partsContainerSchema = new PartsContainerSchema(migrator.SlotDefinitions);
            return partsContainerSchema.Tooltip();
        }

        /// <summary>
        /// Generates tooltip for ModelPartColouring on an item
        /// </summary>
        [TooltipAffectedBy(typeof(ModelPartColourData), nameof(ModelPartColourData.Colour))]
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(ModelPartColourData))]
        public static LocString ModelPartColouringTooltip(this ModelPartColourData colourData) => new TooltipSection(Localizer.DoStr("Colour Data"), colourData.ColourDataTooltip());

        /// <summary>
        /// Tooltip part on a parts container tooltip to show the colour info for one of its installed parts
        /// </summary>
        public static LocString ColourDataTooltip(this ModelPartColourData colourData)
        {
            return Localizer.DoStr("Colour").Style(Text.Styles.Info) + ": " + CopyColourTooltip(colourData.Colour);
        }

        /// <summary>
        /// Shows a coloured box that if you click on will copy the colour's hex code to the clipboard.
        /// </summary>
        public static LocString CopyColourTooltip(Color colour)
        {
            string hexRGB = ColorUtility.RGBHex(colour.HexRGBA);
            return Localizer.NotLocalized($"<mark={ColorUtility.RGBHex(colour.HexRGBA)}>{Text.CopyToClipBoard("Colour", Localizer.NotLocalizedStr(hexRGB), hexRGB)}</mark> ({hexRGB})");
        }

        /// <summary>
        /// Generates a tooltip for PartContainerComponent's persistent data that shows all the slots.
        /// The slots generate their own descriptions about themselves and what part they contain.
        /// </summary>
        [TooltipAffectedBy(typeof(IPartsContainer), nameof(IPartsContainer.NewPartInSlotEvent))]
        [NewTooltip(CacheAs.Instance, 180, overrideType: typeof(IPartsContainer), flags: TTFlags.ClearCacheForAllUsers)]
        public static LocString CurrentPartsListDescription(this IPartsContainer partsContainer)
        {
            List<LocString> slotTooltips = new List<LocString>();
            foreach (ISlot slot in partsContainer.Slots)
            {
                slotTooltips.Add(slot.Tooltip());
            }
            LocString tooltip = slotTooltips.DoubleNewlineList();
            return tooltip;
        }

        /// <summary>
        /// Generate a tooltip section of all the part's properties e.g. colour, storage modifiers etc.
        /// </summary>
        public static LocString PartTooltip(IPart part)
        {
            IEnumerable<object> partProperties = from member in part.GetType().AllMembers()
                                 let memberType = member.GetValueType()
                                 where memberType.DerivesFrom<IPartProperty>()
                                 select member.GetMemberValue(part);

            List<LocString> partTooltip = new List<LocString>();
            foreach (object property in partProperties)
            {
                Type bestTooltipType = GetBestTooltipType(property.GetType());
                if (bestTooltipType != null && TooltipsByType.TryGetValue(bestTooltipType, out Func<object, LocString> tooltipMethod))
                {
                    partTooltip.Add(tooltipMethod(property));
                }
            }
            return partTooltip.NewlineList();
        }
        //TODO: decide a better way to resolve which tooltip to use when multiple types are valid
        public static Type GetBestTooltipType(Type objectType)
        {
            IEnumerable<Type> validTooltipTypes = TooltipsByType.Keys.Where(type => type.IsAssignableFrom(objectType));
            return validTooltipTypes.FirstOrDefault();
        }
    }
}
