using Eco.Core.Controller;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.ComponentModel;

namespace Parts
{
    [Serialized, AutogenClass]
    [UITypeName("PropertyPage")]
    public class SlotView : IController, INotifyPropertyChanged
    {
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay => Slot.Name;

        [SyncToView, Autogen, PropReadOnly, UITypeName("StringDisplay")]
        public string PartName => Slot.Part?.DisplayName;

        [SyncToView, Autogen, AutoRPC, UITypeName("ItemInput")]
        public Inventory SlotInventory
        {
            get => Slot.Inventory; set
            {
                Slot.Inventory = value;
            }
        }
        public Slot Slot { get; init; }

        public SlotView(Slot slot)
        {
            Slot = slot;
            SlotInventory.OnChanged.Add((User user) => Log.WriteLine(Localizer.DoStr($"{user.Name} changed slot inventory")));

            SlotInventory.OnChanged.Add(_ => { this.Changed(nameof(PartName)); this.Changed(nameof(SlotInventory)); });
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
