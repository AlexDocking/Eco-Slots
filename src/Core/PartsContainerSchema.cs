using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    public class PartsContainerSchema : IPartsContainerSchema
    {
        private List<ISlotDefinition> slotSchema = new List<ISlotDefinition>();
        public IReadOnlyList<ISlotDefinition> SlotSchemas => slotSchema;
        private PartsContainerSchema() { }
        public PartsContainerSchema(IEnumerable<ISlotDefinition> slotSchema) => this.slotSchema = slotSchema.ToList();
        public LocString Tooltip()
        {
            List<LocString> slotTooltips = new List<LocString>();
            foreach(ISlotDefinition slot in slotSchema)
            {
                slotTooltips.Add(slot.Tooltip());
            }
            return slotTooltips.NewlineList();
        }
    }
}
