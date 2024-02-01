using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    [Serialized, AutogenClass]
    [UITypeName("PropertyPage")]
    public class SlotViewController : IController, INotifyPropertyChanged
    {
        private readonly string PreventIcon = "\n<icon name=\"ServerErrors\" type=\"nobg\">";
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay
        {
            get
            {
                LocStringBuilder locStringBuilder = new LocStringBuilder();
                locStringBuilder.Append(Slot.Name);

                if (Slot.GenericDefinition.CanPartEverBeAdded && Slot.GenericDefinition.CanPartEverBeRemoved) locStringBuilder.JoinWithSpaceIfNeeded(Localizer.DoStr("[Optional]"));
                if (Slot.Part != null && Slot.GenericDefinition.CanPartEverBeRemoved)
                {
                    Result canRemovePart = Slot.CanRemovePart();
                    if (!canRemovePart)
                    {
                        locStringBuilder.Append(PreventIcon + canRemovePart.Message.Style(Text.Styles.ErrorLight));
                    }
                }
                else
                {
                    Result canAddPart = Slot.CanAcceptAnyPart();
                    if (!canAddPart)
                    {
                        locStringBuilder.Append(PreventIcon + canAddPart.Message.Style(Text.Styles.ErrorLight));
                    }
                }
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
        public string ValidTypesDisplay
        {
            get
            {
                if (Slot?.GenericDefinition.RestrictionsToAddPart.FirstOrDefault(restriction => restriction is LimitedTypeSlotRestriction) is LimitedTypeSlotRestriction limitedTypeSlotRestriction)
                {
                    IEnumerable<Item> allowedItems = limitedTypeSlotRestriction.AllowedTypes.Select(type => Item.Get(type)).NonNull();
                    return Localizer.DoStr("Accepts").AppendSpaceIfSet() + allowedItems.Select(item => item.UILink()).CommaList();
                }
                return "";
            }
        }

        public InventorySlot Slot { get; init; }
        public SlotViewController(InventorySlot slot)
        {
            Slot = slot;
            Slot.NewPartInSlotEvent.Add(() => { this.Changed(nameof(PartName)); this.Changed(nameof(SlotInventory)); });
            Slot.SlotStatusChanged.Add(OnSlotEnabledChanged);
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
