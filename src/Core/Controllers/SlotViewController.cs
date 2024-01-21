using Eco.Core.Controller;
using Eco.Core.Systems;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Items;
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
    public class SlotViewController : IController, INotifyPropertyChanged
    {
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay
        {
            get
            {
                if (!Enabled)
                {
                    return Slot?.Name + (Slot?.PartsContainer?.SlotRestrictionManager?.IsOptional(Slot) ?? true ? " [Optional]" : "")
                        + "\n<icon name=\"ServerErrors\" type=\"nobg\">" + new LocString("Locked until storage is empty").Style(Text.Styles.ErrorLight);
                }
                return Slot?.Name + (Slot?.PartsContainer?.SlotRestrictionManager?.IsOptional(Slot) ?? true ? " [Optional]" : "");
            }
        }

        [SyncToView, Autogen, PropReadOnly, UITypeName("StringTitle")]
        public string PartName => Slot?.Part?.DisplayName;

        [SyncToView, Autogen, AutoRPC, UITypeName("ItemInput")]
        public Inventory SlotInventory
        {
            get => Slot?.Inventory; set
            {
            }
        }
        [SyncToView, Autogen, PropReadOnly, UITypeName("StringTitle")]
        public string ValidTypesDisplay => Slot?.PartsContainer?.SlotRestrictionManager?.DisplayRestriction(Slot).NotTranslated;
        private bool Enabled => Slot.PartsContainer.SlotRestrictionManager.IsSlotEnabled(Slot);
        public Slot Slot { get; init; }

        public SlotViewController(Slot slot)
        {
            Slot = slot;
            Slot.NewPartInSlotEvent.Add(() => { this.Changed(nameof(PartName)); this.Changed(nameof(SlotInventory)); });
            Slot.PartsContainer.SlotRestrictionManager.SlotEnabledChangedEvent.Add(OnSlotEnabledChanged);
        }
        private void OnSlotEnabledChanged(Slot slot)
        {
            if (slot != Slot) return;
            this.Changed(nameof(Enabled));
            this.Changed(nameof(SlotInventory));
            this.Changed(nameof(NameDisplay));
        }

        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
