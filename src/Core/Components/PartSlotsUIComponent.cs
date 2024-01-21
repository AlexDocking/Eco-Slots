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
        private IList<(Slot, SlotViewController)> slotViews = new ThreadSafeList<(Slot, SlotViewController)>();
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
            IReadOnlyList<Slot> slots = PartsContainer.Slots;

            for (int i = 0; i < slots.Count; i++)
            {
                Slot slot = slots[i];
                SlotViewController slotView = new SlotViewController(slot);
                slotViews.Add((slot, slotView));
                slot.NewPartInSlotEvent.Add(PartsUI.NotifyChanged);
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
