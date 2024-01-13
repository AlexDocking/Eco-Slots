using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System.ComponentModel;
using System.Linq;
using static Eco.Gameplay.Items.AuthorizationInventory;

namespace Parts
{
    [Serialized]
    public class Slot : IController, INotifyPropertyChanged
    {
        [SyncToView] public string Name { get; set; }
        [Serialized, SyncToView, NewTooltip(Eco.Shared.Items.CacheAs.Disabled)] public Inventory Inventory { get; set; } = new AuthorizationInventory(1);

        [SyncToView]
        public IPart Part => Inventory.NonEmptyStacks?.FirstOrDefault()?.Item as IPart;

        public ThreadSafeAction OnPartChanged { get; } = new ThreadSafeAction();
        public void Initialize(WorldObject worldObject)
        {
            if (this.Inventory is not AuthorizationInventory)
            {
                // ensure the inventory type is authorization inventory (migration)
                var newInventory = new AuthorizationInventory(
                    this.Inventory.Stacks.Count(),
                    AuthorizationFlags.AuthedMayAdd | AuthorizationFlags.AuthedMayRemove);
                newInventory.ReplaceStacks(this.Inventory.Stacks);
                this.Inventory = newInventory;
            }
            this.Inventory.SetOwner(worldObject);
            this.Inventory.OnChanged.Add(OnInventoryChanged);
            Log.WriteLine(Localizer.DoStr($"Initialize slot {Name} with object {worldObject?.Name}"));
        }
        private void OnInventoryChanged(User user) => OnPartChanged.Invoke();

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
