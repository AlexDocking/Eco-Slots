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
                new SlotDefinition()
                {
                    Name = "Unit",
                    Optional = false,
                    AllowedItemTypes = new[]
                    {
                        typeof(KitchenBaseCabinetBoxItem)
                    },
                    MustHavePartIfEmpty = () => new KitchenBaseCabinetBoxItem(),
                },
                new SlotDefinition()
                {
                    Name = "Worktop",
                    Optional = false,
                    AllowedItemTypes = new[]
                    {
                        typeof(KitchenCupboardWorktopItem)
                    },
                    MustHavePartIfEmpty = () => new KitchenCupboardWorktopItem(),
                },
                new SlotDefinition()
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
