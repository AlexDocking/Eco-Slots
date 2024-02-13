using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Vehicles
{
    public class TruckMigrator : DefaultPartsContainerMigrator
    {
        public TruckMigrator() : base()
        {
            SlotDefinitions = new SlotDefinitions()
            {
                new DefaultInventorySlotDefinition()
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
