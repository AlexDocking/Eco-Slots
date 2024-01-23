using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Kitchen
{
    public class KitchenBaseCabinetSchema : RegularSchema
    {
        public KitchenBaseCabinetSchema(WorldObject worldObject) : base()
        {
            SlotDefinitions = new SlotDefinitions()
            {
                new RegularSlotDefinition()
                {
                    Name = "Unit",
                    Optional = false,
                    AllowedItemTypes = new[]
                    {
                        typeof(KitchenBaseCabinetBoxItem)
                    },
                    MustHavePartIfEmpty = () => new KitchenBaseCabinetBoxItem(),
                },
                new RegularSlotDefinition()
                {
                    Name = "Worktop",
                    Optional = false,
                    AllowedItemTypes = new[]
                    {
                        typeof(KitchenCupboardWorktopItem)
                    },
                    MustHavePartIfEmpty = () => new KitchenCupboardWorktopItem(),
                },
                new RegularSlotDefinition()
                {
                    Name = "Door",
                    Optional = true,
                    AllowedItemTypes = new[]
                    {
                        typeof(KitchenCabinetFlatDoorItem),
                        typeof(KitchenCupboardRaisedPanelDoorItem)
                    },
                }
            };
        }
    }
}
