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

namespace Parts.UI
{
    /// <summary>
    /// Presents a view for an InventorySlot, which can be shown in a list by the PartSlotsUIComponent.
    /// It looks like:
    /// <code>
    /// Slot Name
    /// Part Name
    /// [Inventory]
    /// Description of valid part types</code>
    /// </summary>
    [Serialized, AutogenClass]
    [UITypeName("PropertyPage")]
    public sealed class InventorySlotController : IController, INotifyPropertyChanged
    {
        //The 'prevent' icon with the hand in the red octogon
        private static string PreventIcon => Text.Icon("ServersError", "", "nobg");
        [SyncToView, Autogen]
        [UITypeName("GeneralHeader")]
        public string NameDisplay
        {
            get
            {
                LocStringBuilder locStringBuilder = new LocStringBuilder();
                locStringBuilder.Append(Slot.Name);

                if (Slot.SlotDefinition.CanPartEverBeAdded && Slot.SlotDefinition.CanPartEverBeRemoved) locStringBuilder.JoinWithSpaceIfNeeded(Localizer.DoStr("[Optional]"));
                if (Slot.Part != null && Slot.SlotDefinition.CanPartEverBeRemoved)
                {
                    Result canRemovePart = Slot.CanRemovePart();
                    if (!canRemovePart)
                    {
                        locStringBuilder.AppendLine();
                        locStringBuilder.Append(PreventIcon + canRemovePart.Message.Style(Text.Styles.ErrorLight));
                    }
                }
                else
                {
                    Result canAddPart = Slot.CanAcceptAnyPart();
                    if (!canAddPart)
                    {
                        locStringBuilder.AppendLine();
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
                if (Slot?.SlotDefinition.RestrictionsToAddPart.FirstOrDefault(restriction => restriction is LimitedTypeSlotRestriction) is LimitedTypeSlotRestriction limitedTypeSlotRestriction)
                {
                    IEnumerable<Item> allowedItems = limitedTypeSlotRestriction.AllowedTypes.Select(type => Item.Get(type)).NonNull();
                    return Localizer.DoStr("Accepts").AppendSpaceIfSet() + allowedItems.Select(item => item.UILink()).CommaList();
                }
                return "";
            }
        }

        public InventorySlot Slot { get; init; }
        public InventorySlotController(InventorySlot slot)
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
