using Eco.Gameplay.Components.Storage;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Vehicles
{
    public class TruckSchema : RegularSchema
    {
        public TruckSchema(WorldObject worldObject) : base()
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
                    StoragesThatMustBeEmpty = new[]
                    {
                        worldObject.GetComponent<PublicStorageComponent>().Storage
                    }
                }
            };
        }
    }
}
