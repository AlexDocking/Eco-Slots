using Eco.Core.Controller;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Serialization;

namespace Parts
{
    [Serialized]
    public class Slot
    {
        [SyncToView] public string Name { get; set; }
        [Serialized, SyncToView, NewTooltip(Eco.Shared.Items.CacheAs.Disabled)] public Inventory Inventory { get; set; } = new AuthorizationInventory(1);
    }
}
