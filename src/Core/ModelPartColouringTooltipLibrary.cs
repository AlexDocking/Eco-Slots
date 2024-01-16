using Eco.Core.Controller;
using Eco.Core.PropertyHandling;
using Eco.Core.Serialization.Migrations;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.IoC;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using KitchenUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    [TooltipLibrary]
    public static class ModTooltipLibrary
    {
        public static ThreadSafeDictionary<Type, Func<object, LocString>> TooltipsByType { get; } = new ThreadSafeDictionary<Type, Func<object, LocString>>();
        public static void Initialize()
        {
            TooltipsByType.Add(typeof(ModelPartColouring), o => ModelPartColouringTooltip(o as ModelPartColouring));
            ModelPartColouring.OnColourChangedGlobal.Add(colouring => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(ModelPartColourComponentTooltip), typeof(IHasModelPartColourComponent), colouring));

            ModelPartColouring.OnColourChangedGlobal.Add(colouring => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(ModelPartColouringTooltip), typeof(ModelPartColouring), colouring));
            PartsContainer.OnPartChangedGlobal.Add(partsContainer => ServiceHolder<ITooltipSubscriptions>.Obj.MarkTooltipPartDirty(nameof(CurrentPartsListDescription), null, partsContainer));
        }
        /// <summary>
        /// Generates tooltip on items which derive IHasModelPartColourComponent
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        [TooltipAffectedBy(typeof(IHasModelPartColourComponent), nameof(IHasModelPartColourComponent.ColourData), nameof(ModelPartColouring.Colour))]
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(IHasModelPartColourComponent))]
        public static LocString ModelPartColourComponentTooltip(this IHasModelPartColourComponent part) => part != Item.Get(part.GetType()) ? new TooltipSection(part.ColourData.ModelPartColouringTooltip()) : LocString.Empty;
        
        /// <summary>
        /// Generates tooltip for ModelPartColouring
        /// </summary>
        /// <param name="colourData"></param>
        /// <returns></returns>
        [TooltipAffectedBy(typeof(ModelPartColouring), nameof(ModelPartColouring.Colour))]
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(ModelPartColouring))]
        public static LocString ModelPartColouringTooltip(this ModelPartColouring colourData) => new TooltipSection(Localizer.DoStr("Colour Data"), Localizer.DoStr(ColorUtility.RGBHex(colourData.Colour.HexRGBA)));

        private static ThreadSafeHashSet<(User, PartsContainer)> ExpandedContainers { get; } = new ThreadSafeHashSet<(User, PartsContainer)>();
        

        /// <summary>
        /// Generates tooltip for PartsContainer
        /// </summary>
        /// <param name="partsContainer"></param>
        /// <returns></returns>
        //[TooltipAffectedBy(typeof(PartsContainer), nameof(PartsContainer.OnPartChanged))]
        [NewTooltip(CacheAs.Instance, 150, overrideType: typeof(PartsContainer))]//, TooltipAffectedBy(nameof(OnPartChanged))]
        public static LocString CurrentPartsListDescription(this PartsContainer partsContainer)
        {
            Log.WriteLine(Localizer.DoStr("CurrentPartsListDescription"));
            List<LocString> partTooltips = new List<LocString>
            {
                Localizer.DoStr("Contains parts:")
            };
            foreach (Slot slot in partsContainer.Slots)
            {
                IPart part = slot.Part;
                ILinkable linkable = part as ILinkable;
                LocString content;
                if (linkable != null) content = linkable.UILink(Localizer.DoStr(part.DisplayName).Style(Text.Styles.Item)).AppendLine(PartTooltip(part));
                else content = Localizer.DoStr("Empty.").AppendLine(slot.PartsContainer.SlotRestrictionManager.DisplayRestriction(slot));
                partTooltips.Add(new TooltipSection(Localizer.DoStr("Slot") + ": " + slot.Name, content));
                
            }
            LocString tooltip = partTooltips.NewlineList();
            Log.WriteLine(tooltip);
            return tooltip;
        }

        private static LocString PartTooltip(IPart part)
        {
            Localizer.DoStr("PartTooltip:" + part.DisplayName);
            var partProperties = from member in part.GetType().AllMembers()
                                 let memberType = member.GetValueType()
                                 where memberType.DerivesFrom<IPartProperty>()
                                 select member.GetMemberValue(part);

            List<LocString> partTooltip = new List<LocString>();
            foreach (object property in partProperties)
            {
                Log.WriteLine(Localizer.DoStr($"property " + property.GetType()));
                if (TooltipsByType.TryGetValue(property.GetType(), out Func<object, LocString> tooltipMethod))
                {
                    partTooltip.Add(tooltipMethod(property));
                }
            }
            Log.WriteLine(Localizer.DoStr($"Found {partTooltip.Count()} tooltip objects"));
            return partTooltip.NewlineList();
        }
    }
}
