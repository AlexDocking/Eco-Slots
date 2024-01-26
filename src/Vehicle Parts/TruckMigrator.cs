using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Vehicles
{
    public class TruckMigrator : RegularPartsContainerMigrator
    {
        public TruckMigrator() : base()
        {
            SlotDefinitions = new SlotDefinitions()
            {
                new RegularSlotDefinition()
                {
                    Name = "Storage Reinforcement",
                    Optional = true,
                    AllowedItemTypes = new[]
                    {
                        typeof(StandardTruckBedItem),
                        typeof(BigTruckBedItem),
                    },
                    RequiresEmptyStorageToChangePart = true
                }
            };
        }
    }
}
