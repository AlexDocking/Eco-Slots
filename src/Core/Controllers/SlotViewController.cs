using Eco.Core.Controller;
using Eco.Gameplay.Items;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
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
                LocStringBuilder locStringBuilder = new LocStringBuilder();
                locStringBuilder.Append((string)Slot.Name);

                if (SlotRestrictionManager.IsOptional(Slot)) locStringBuilder.JoinWithSpaceIfNeeded(Localizer.DoStr("[Optional]"));
                if (SlotRestrictionManager.IsSlotLocked(Slot)) locStringBuilder.Append("\n<icon name=\"ServerErrors\" type=\"nobg\">" + new LocString("Locked until storage is empty").Style(Text.Styles.ErrorLight));
                return locStringBuilder.ToLocString();
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
        public InventorySlot Slot { get; init; }
        private ISlotRestrictionManager SlotRestrictionManager => Slot?.PartsContainer?.SlotRestrictionManager;
        public SlotViewController(InventorySlot slot)
        {
            Slot = slot;
            Slot.NewPartInSlotEvent.Add(() => { this.Changed(nameof(PartName)); this.Changed(nameof(SlotInventory)); });
            Slot.PartsContainer.SlotRestrictionManager.SlotLockedChangedEvent.Add(OnSlotEnabledChanged);
        }
        private void OnSlotEnabledChanged(ISlot slot)
        {
            if (slot != Slot) return;
            this.Changed(nameof(Locked));
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
