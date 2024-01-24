using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    [Serialized]
    [AutogenClass]
    [CreateComponentTabLoc("Slots", true)]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    public class PartSlotsUIComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        private IList<(ISlot, SlotViewController)> slotViews = new ThreadSafeList<(ISlot, SlotViewController)>();
        private IEnumerable<SlotViewController> Viewers => slotViews.Select(pair => pair.Item2);
        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<SlotViewController> PartsUI { get; private set; }

        private IPartsContainer PartsContainer { get; set; }
        public PartSlotsUIComponent()
        {
            PartsUI = new ControllerList<SlotViewController>(this, nameof(PartsUI), Array.Empty<SlotViewController>());
        }
        public override void Initialize()
        {
            base.Initialize();
            PartsContainer = Parent.GetComponent<PartsContainerComponent>().PartsContainer;
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            IReadOnlyList<ISlot> slots = PartsContainer.Slots;

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i] as InventorySlot;
                if (slot != null)
                {
                    SlotViewController slotView = new SlotViewController(slot);
                    slotViews.Add((slot, slotView));
                    slot.NewPartInSlotEvent.Add(PartsUI.NotifyChanged);
                }
            }
            PartsUI.Set(Viewers);
            PartsUI.Callbacks.OnAdd.Add(ResetList);
            PartsUI.Callbacks.OnRemove.Add(ResetList);
        }
        private void ResetList(INetObject sender, object obj) => ResetList();
        private void ResetList()
        {
            PartsUI.Set(Viewers);
            PartsUI.NotifyChanged();
        }
    }
}
