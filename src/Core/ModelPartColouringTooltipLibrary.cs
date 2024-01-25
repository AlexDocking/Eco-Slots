using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.IoC;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Controller;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Shared.Localization.ConstLocs;

namespace Parts
{
    [TooltipLibrary]
    public static class ModTooltipLibrary
    {
        public static ThreadSafeDictionary<Type, Func<object, LocString>> TooltipsByType { get; } = new ThreadSafeDictionary<Type, Func<object, LocString>>();
        public static void Initialize()
        {
            TooltipsByType.Add(typeof(ModelPartColouring), o => (o as ModelPartColouring).ColourDataTooltip());
            TooltipsByType.Add(typeof(ICustomStorageSize), o => (o as ICustomStorageSize).CustomStorageSizeTooltip());
            PartNotifications.PartPropertyChangedEventGlobal.Add((part, property) => { if (part is IController controller) ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(ModelPartColourComponentTooltip), null, controller); });

            ModelPartColouring.OnColourChangedGlobal.Add(colouring => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(ModelPartColouringTooltip), null, colouring));
            PartsContainer.PartsContainerChangedEventGlobal.Add(partsContainer
                => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(CurrentPartsListDescription), null, partsContainer));
        }
        /// <summary>
        /// Generates tooltip on items which derive IHasModelPartColourComponent
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        [TooltipAffectedBy(typeof(IHasModelPartColour), nameof(IHasModelPartColour.ColourData), nameof(ModelPartColouring.Colour))]
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(IHasModelPartColour))]
        public static LocString ModelPartColourComponentTooltip(this IHasModelPartColour part) => part != Item.Get(part.GetType()) ? new TooltipSection(part.ColourData.ModelPartColouringTooltip()) : LocString.Empty;

        /// <summary>
        /// Generates tooltip on items which derive IHasCustomStorageSize
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        [TooltipAffectedBy(typeof(IHasCustomStorageSize), nameof(IHasCustomStorageSize.StorageSizeModifier), nameof(ICustomStorageSize.NumberOfAdditionalSlots))]
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(IHasCustomStorageSize))]
        public static LocString CustomStorageSizeComponentTooltip(this IHasCustomStorageSize part) => new TooltipSection(Localizer.DoStr("Storage Size"), part.StorageSizeModifier.CustomStorageSizeTooltip());


        /// <summary>
        /// Tooltip part on a parts container to show the colour info for one of its installed parts
        /// </summary>
        /// <param name="colourData"></param>
        /// <returns></returns>
        public static LocString CustomStorageSizeTooltip(this ICustomStorageSize customStorageSize)
        {
            return Localizer.Do($"Increases the number of storage slots by {Text.Info(customStorageSize.NumberOfAdditionalSlots)}").Style(Text.Styles.Info);
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
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(IPartsContainer), flags: TTFlags.ClearCacheForAllUsers)]
        public static LocString CurrentPartsListDescription(this IPartsContainer partsContainer)
        {
            List<LocString> partTooltips = new List<LocString>
            {
                Localizer.DoStr("Contains parts:").Style(Text.Styles.Title)
            };
            foreach (ISlot slot in partsContainer.Slots)
            {
                IPart part = slot.Part;
                ILinkable linkable = part as ILinkable;
                LocString content;
                if (linkable != null) content = linkable.UILink(Localizer.DoStr(part.DisplayName).Style(Text.Styles.Name)).AppendLine(PartTooltip(part));
                else if (part != null) content = Localizer.DoStr(part.DisplayName).Style(Text.Styles.Name).AppendLine(PartTooltip(part));
                else
                {
                    LocStringBuilder contentStringBuilder = new LocStringBuilder();
                    var restrictions = slot.GenericDefinition.RestrictionsToAddPart.ToList();
                    if (restrictions.FirstOrDefault(restriction => restriction is LimitedTypeSlotRestriction) is LimitedTypeSlotRestriction limitedTypeSlotRestriction)
                    {
                        contentStringBuilder.AppendLine(Localizer.DoStr("Can be") + " " + limitedTypeSlotRestriction.AllowedTypes.Select(type => type.UILink()).CommaList(CommonLocs.None, CommonLocs.Or));
                        restrictions.Remove(limitedTypeSlotRestriction);
                    }
                    else
                    {
                    }
                    IEnumerable<LocString> restrictionDescriptions = restrictions.Select(restriction => restriction.Describe());
                    if (restrictionDescriptions.Any())
                    {
                        contentStringBuilder.AppendLine(Localizer.DoStr("Requirements:"));
                        contentStringBuilder.AppendLine(restrictionDescriptions.TextList($"\n{CommonLocs.And}\n"));
                    }

                    content = contentStringBuilder.ToLocString();
                }
                partTooltips.Add((Localizer.DoStr("Slot") + ": " + slot.Name).Style(Text.Styles.Header).AppendLine(content));
            }
            LocString tooltip = partTooltips.DoubleNewlineList();
            return tooltip;
        }

        private static LocString PartTooltip(IPart part)
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
