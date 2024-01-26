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
        private IList<(ISlot, object)> slotViews = new ThreadSafeList<(ISlot, object)>();
        public IEnumerable<object> Views => slotViews.Select(pair => pair.Item2);
        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<object> PartsUI { get; private set; }

        public SlotViewCreator ViewCreator { get; set; } = new SlotViewCreator();
        public PartSlotsUIComponent()
        {
            PartsUI = new ControllerList<object>(this, nameof(PartsUI), Array.Empty<object>());
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            CreateViews(Parent.GetComponent<PartsContainerComponent>().PartsContainer);
            PartsUI.Callbacks.OnAdd.Add(ResetList);
            PartsUI.Callbacks.OnRemove.Add(ResetList);
        }
        public void CreateViews(IPartsContainer partsContainer)
        {
            IReadOnlyList<ISlot> slots = partsContainer.Slots;
            foreach(ISlot slot in slotViews.Select(sv => sv.Item1))
            {
                slot.NewPartInSlotEvent.Remove(OnNewPartInSlot);
            }
            slotViews.Clear();
            for (int i = 0; i < slots.Count; i++)
            {
                ISlot slot = slots[i];
                if (slot != null)
                {
                    object slotView = ViewCreator.CreateView(slot);
                    if (slotView != null)
                    {
                        slotViews.Add((slot, slotView));
                        slot.NewPartInSlotEvent.Add(OnNewPartInSlot);
                    }
                }
            }
            ResetList();
        }
        private void ResetList(INetObject sender, object obj) => ResetList();
        private void OnNewPartInSlot() => PartsUI.NotifyChanged();
        private void ResetList()
        {
            PartsUI.Set(Views);
            PartsUI.NotifyChanged();
        }
    }
}
