using Eco.Gameplay.Objects;
using Eco.Shared.Utils;
using Parts.Migration;
using System.Collections.Generic;
using System.Linq;

namespace Parts.Kitchen
{
    public class KitchenBaseCabinetMigrator: DefaultPartsContainerMigrator
    {
        public KitchenBaseCabinetMigrator() : base()
        {
            SlotDefinitions = new SlotDefinitions()
            {
                new DefaultInventorySlotDefinition()
                {
                    Name = "Unit",
                    Optional = false,
                    AllowedItemTypes = new[]
                    {
                        typeof(KitchenBaseCabinetBoxItem)
                    },
                    MustHavePartIfEmpty = () => new KitchenBaseCabinetBoxItem(),
                },
                new DefaultInventorySlotDefinition()
                {
                    Name = "Worktop",
                    Optional = false,
                    AllowedItemTypes = new[]
                    {
                        typeof(KitchenCupboardWorktopItem)
                    },
                    MustHavePartIfEmpty = () => new KitchenCupboardWorktopItem(),
                },
                new DefaultInventorySlotDefinition()
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
