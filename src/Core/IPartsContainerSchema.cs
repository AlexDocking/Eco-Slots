using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    public interface IPartsContainerSchema
    {
        IReadOnlyList<ISlotDefinition> SlotSchemas { get; }
        LocString Tooltip();
    }
}
