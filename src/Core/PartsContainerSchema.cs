using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    /// <summary>
    /// A collection of slot definitions. The slots for a new world object will be created based on this.
    /// </summary>
    public interface IPartsContainerSchema
    {
        IReadOnlyList<ISlotDefinition> SlotDefinitions { get; }
        /// <summary>
        /// Describe all the slots the WorldObject would have when placed down.
        /// </summary>
        /// <returns></returns>
        LocString Tooltip();
    }
    /// <summary>
    /// This defines the definitions for each slot, so that tooltips know what slots there are and what they accept, even on the tooltips for world object items.
    /// It is also used to create the slots for a world object based on this schema
    /// </summary>
    public class PartsContainerSchema : IPartsContainerSchema
    {
        private List<ISlotDefinition> slotSchemas = new List<ISlotDefinition>();
        public IReadOnlyList<ISlotDefinition> SlotDefinitions => slotSchemas;
        private PartsContainerSchema() { }
        public PartsContainerSchema(IEnumerable<ISlotDefinition> slotSchemas) => this.slotSchemas = slotSchemas.ToList();
        public LocString Tooltip()
        {
            List<LocString> slotTooltips = new List<LocString>();
            foreach(ISlotDefinition slot in slotSchemas)
            {
                slotTooltips.Add(slot.TooltipTitle().AppendLine(slot.TooltipContent()));
            }
            if (slotTooltips.Count == 0) return LocString.Empty;
            LocStringBuilder tooltipBuilder = new LocStringBuilder();

            tooltipBuilder.AppendLine(slotTooltips.NewlineList());
            return tooltipBuilder.ToLocString();
        }
    }
}
