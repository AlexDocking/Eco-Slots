using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Parts
{
    public class PartsContainerSchema : IPartsContainerSchema
    {
        private List<ISlotDefinition> slotSchemas = new List<ISlotDefinition>();
        public IReadOnlyList<ISlotDefinition> SlotSchemas => slotSchemas;
        private PartsContainerSchema() { }
        public PartsContainerSchema(IEnumerable<ISlotDefinition> slotSchemas) => this.slotSchemas = slotSchemas.ToList();
        public LocString Tooltip()
        {
            List<LocString> slotTooltips = new List<LocString>();
            foreach(ISlotDefinition slot in slotSchemas)
            {
                slotTooltips.Add(slot.Tooltip());
            }
            return slotTooltips.NewlineList();
        }
    }
}
