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
    [TooltipLibrary]
    public static class ModTooltipLibrary
    {
        public static ThreadSafeDictionary<Type, Func<object, LocString>> TooltipsByType { get; } = new ThreadSafeDictionary<Type, Func<object, LocString>>();
        //public static ThreadSafeDictionary<Type, Func<object, LocString>> GenericTooltipsByType { get; } = new ThreadSafeDictionary<Type, Func<object, LocString>>();
        public static void Initialize()
        {
            TooltipsByType.Add(typeof(ModelPartColouring), o => (o as ModelPartColouring).ColourDataTooltip());
            TooltipsByType.Add(typeof(ICustomStorageSize), o => (o as ICustomStorageSize).CustomStorageSizeTooltip());
            //TooltipsByType.Add(typeof(IHasCustomStorageSize), o => (o as IHasCustomStorageSize).CustomStorageSizeComponentTooltip());

            //GenericTooltipsByType.Add(typeof(IHasCustomStorageSize), o => SpecificCustomStorageSizeComponentTooltip((o as IHasCustomStorageSize)));
            //GenericTooltipsByType.Add(typeof(IHasModelPartColour), o => GenericHasModelPartColourTypeTooltip((o as IHasModelPartColour)));

            PartNotifications.PartPropertyChangedEventGlobal.Add((part, property) => { if (part is IController controller) ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(SpecificHasModelPartColourTooltip), null, controller); });

            ModelPartColouring.OnColourChangedGlobal.Add(colouring => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(ModelPartColouringTooltip), null, colouring));
            PartsContainer.PartsContainerChangedEventGlobal.Add(partsContainer
                => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(CurrentPartsListDescription), null, partsContainer));
            PartsContainer.PartsContainerChangedEventGlobal.Add(partsContainer
                => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(GenericPartsContainerWorldObjectItemTooltip), null, partsContainer)); 
        }
        /// <summary>
        /// Generates tooltip on items which derive IHasModelPartColourComponent
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        [TooltipAffectedBy(typeof(IHasModelPartColour), nameof(IHasModelPartColour.ColourData), nameof(ModelPartColouring.Colour))]
        [NewTooltip(CacheAs.Instance, 20, overrideType: typeof(IHasModelPartColour))]
        public static LocString SpecificHasModelPartColourTooltip(this IHasModelPartColour part) => part != Item.Get(part.GetType()) ? new TooltipSection(part.ColourData.ModelPartColouringTooltip()) : LocString.Empty;

        /*[NewTooltip(CacheAs.Instance, 20, overrideType: typeof(IHasModelPartColour))]
        public static LocString GenericHasModelPartColourTypeTooltip(this IHasModelPartColour part)
        {
            if (part != Item.Get(part.GetType())) return LocString.Empty;
            return new TooltipSection(Localizer.DoStr("Colour"), Localizer.DoStr("This part can be coloured"));
        }*/

        /// <summary>
        /// Generates tooltip on items which derive IHasCustomStorageSize
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(IHasCustomStorageSize))]
        public static LocString GenericCustomStorageSizeComponentTooltip(Type partType)
        {
            if (Item.Get(partType) is not IHasCustomStorageSize part) return LocString.Empty;
            //if (part != Item.Get(part.GetType())) return LocString.Empty;
            return new TooltipSection(Localizer.DoStr("Storage Size"), part.StorageSizeModifier.CustomStorageSizeTooltip());
        }

        /*[NewTooltip(CacheAs.Instance, 20, overrideType: typeof(IPart))]
        public static LocString PartTypeTooltip(this IPart part)
        {
            Log.WriteLine(Localizer.DoStr("Looking for part " + part.DisplayName));
            if (part is Item && part != Item.Get(part.GetType())) return LocString.Empty;
            //if (type.DerivesFrom(typeof(IPart)) && type.DerivesFrom(typeof(Item)))
            {
                //IPart part = (IPart)Item.Get(type);
                Log.WriteLine(Localizer.DoStr("Found part type tooltip:" + part.DisplayName + "," + (part != null)));

                LocStringBuilder tooltipBuilder = new LocStringBuilder();
                foreach(var partWithPropertyInterface in part.GetType().GetInterfaces().Where(interfaceType => interfaceType.DerivesFrom(typeof(IPart)) && interfaceType != typeof(IPart)))
                {
                    Log.WriteLine(Localizer.DoStr("Found property:" + partWithPropertyInterface.Name));
                    if (TooltipsByType.TryGetValue(partWithPropertyInterface, out Func<object, LocString> tooltipMethod))
                    {
                        tooltipBuilder.AppendLine(tooltipMethod(part));
                    }
                }
                return tooltipBuilder.ToLocString();
            }
            return LocString.Empty;
        }*/

        /// <summary>
        /// Tooltip part on a parts container to show the colour info for one of its installed parts
        /// </summary>
        /// <param name="colourData"></param>
        /// <returns></returns>
        public static LocString CustomStorageSizeTooltip(this ICustomStorageSize customStorageSize)
        {
            return Localizer.Do($"Increases the number of storage slots by {Text.Info(customStorageSize.NumberOfAdditionalSlots)}").Style(Text.Styles.Info);
        }

        [NewTooltip(CacheAs.Instance, 180, overrideType: typeof(IPartsContainerWorldObject))]
        public static LocString GenericPartsContainerWorldObjectItemTooltip(this IPartsContainerWorldObject partsContainerItem)
        {
            if (partsContainerItem != Item.Get(partsContainerItem.GetType()))
            {
                ItemPersistentData persistentData = (partsContainerItem as IPersistentData)?.PersistentData as ItemPersistentData;
                if (persistentData != null && persistentData.TryGetPersistentData<PartsContainerComponent>(out object partsContainerPersistentData))
                {
                    return LocString.Empty;
                }
            }
            //if (worldObjectType.DerivesFrom(typeof(IPartsContainerWorldObject)) && worldObjectType.DerivesFrom(typeof(Item)))
            {
                //IPartsContainerWorldObject partsContainerItem = (IPartsContainerWorldObject)Item.Get(worldObjectType);
                //ItemPersistentData persistentData = (partsContainerItem as IPersistentData)?.PersistentData as ItemPersistentData;
                //if (persistentData.TryGetPersistentData<PartsContainerComponent>(out object partsContainer) && partsContainer != null) return LocString.Empty;
                if (partsContainerItem.GetPartsContainerMigrator() is not RegularPartsContainerMigrator migrator) return LocString.Empty;
                IPartsContainerSchema partsContainerSchema = new PartsContainerSchema(migrator.SlotDefinitions);
                return partsContainerSchema.Tooltip();
            }
            //return LocString.Empty;
        }

        /// <summary>
        /// Generates tooltip for ModelPartColouring on an item
        /// </summary>
        /// <param name="colourData"></param>
        /// <returns></returns>
        [TooltipAffectedBy(typeof(ModelPartColouring), nameof(ModelPartColouring.Colour))]
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(ModelPartColouring))]
        public static LocString ModelPartColouringTooltip(this ModelPartColouring colourData) => new TooltipSection(Localizer.DoStr("Colour Data"), colourData.ColourDataTooltip());

        /// <summary>
        /// Tooltip part on a parts container to show the colour info for one of its installed parts
        /// </summary>
        /// <param name="colourData"></param>
        /// <returns></returns>
        public static LocString ColourDataTooltip(this ModelPartColouring colourData)
        {
            return Localizer.DoStr("Colour").Style(Text.Styles.Info) + ": " + CopyColourTooltip(colourData.Colour);
        }

        private static LocString CopyColourTooltip(Color colour)
        {
            string hexRGB = ColorUtility.RGBHex(colour.HexRGBA);
            return Localizer.NotLocalized($"<mark={ColorUtility.RGBHex(colour.HexRGBA)}>{Text.CopyToClipBoard("Colour", Localizer.NotLocalizedStr(hexRGB), hexRGB)}</mark> ({hexRGB})");
        }

        /// <summary>
        /// Generates tooltip for PartsContainer
        /// </summary>
        /// <param name="partsContainer"></param>
        /// <returns></returns>
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

        public static LocString PartTooltip(IPart part)
        {
            var partProperties = from member in part.GetType().AllMembers()
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
