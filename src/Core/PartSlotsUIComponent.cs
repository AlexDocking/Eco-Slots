using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Utils;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    [Serialized]
    [AutogenClass]
    [CreateComponentTabLoc("Slots")]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    public class PartSlotsUIComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        private IList<(Slot, SlotView)> slotViews = new ThreadSafeList<(Slot, SlotView)>();
        private IEnumerable<SlotView> Viewers => slotViews.Select(pair => pair.Item2);
        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<SlotView> PartsUI { get; private set; }

        private PartsContainer PartsContainer { get; set; }
        public PartSlotsUIComponent()
        {
            PartsUI = new ControllerList<SlotView>(this, nameof(PartsUI), Array.Empty<SlotView>());
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
                SlotView slotView = new SlotView(slot);
                slotViews.Add((slot, slotView));
                slot.OnPartChanged.Add(PartsUI.NotifyChanged);
            }
            PartsUI.Set(Viewers);
            PartsUI.Callbacks.OnAdd.Add(ResetList);
            PartsUI.Callbacks.OnRemove.Add(ResetList);
        }
        private void ResetList(INetObject sender, object obj)
        {
            PartsUI.Set(Viewers);
            PartsUI.NotifyChanged();
        }
    }
}
