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

namespace Parts.UI
{
    /// <summary>
    /// Autogen UI component to display all the slots in a list.
    /// Different types of slot can have different views, as created by the SlotViewCreator.
    /// TODO: refactor this and <see cref="PartColoursUIComponent"/> since they are practically identical in how they work.
    /// TODO: maybe allow mods to swap out the UI for a slot at runtime.
    /// </summary>
    [Serialized]
    [AutogenClass]
    [CreateComponentTabLoc("Slots", true)]
    [NoIcon]
    [RequireComponent(typeof(PartsContainerComponent))]
    public class SlotsUIComponent : WorldObjectComponent, IHasClientControlledContainers, INotifyPropertyChanged
    {
        private IList<(ISlot, object)> slotViews = new ThreadSafeList<(ISlot, object)>();
        /// <summary>
        /// List of the UI view for each slot.
        /// </summary>
        public IEnumerable<object> Views => slotViews.Select(pair => pair.Item2);
        /// <summary>
        /// The autogen UI to show the slot views in a list.
        /// The autogen type used is not designed for fixed-length lists, so the player may try to remove views from the list, in which case we need to set this back to what it should be.
        /// The list cannot be marked read only because that also makes every view inside it read only, which is definitely not what we want since the purpose of this is to allow players to add and remove parts from slots, not remove the UI for the slots.
        /// </summary>
        [Autogen, SyncToView, HideRoot, HideRootListEntry]
        public ControllerList<object> PartsUI { get; private set; }

        /// <summary>
        /// Delegate the selection and creation of the view for each slot to this object.
        /// </summary>
        public SlotViewFactory ViewCreator { get; set; } = new SlotViewFactory();
        public SlotsUIComponent()
        {
            PartsUI = new ControllerList<object>(this, nameof(PartsUI), Array.Empty<object>());
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            CreateViews(Parent.GetComponent<PartsContainerComponent>().PartsContainer);
            //The HideRoot view attribute on the PartsUI should prevent the plus icon being visible and thus prevent players adding to the list, so this is just a safety measure.
            PartsUI.Callbacks.OnAdd.Add(ResetList);
            //The delete button on each view within the list cannot be hidden so we must immediately reset the list should the player click it.
            PartsUI.Callbacks.OnRemove.Add(ResetList);
        }
        /// <summary>
        /// Create a view for each slot and add it to the list.
        /// Not all slots may have a UI, in which case no UI is created for it.
        /// </summary>
        /// <param name="partsContainer"></param>
        public void CreateViews(IPartsContainer partsContainer)
        {
            IReadOnlyList<ISlot> slots = partsContainer.Slots;
            foreach (ISlot slot in slotViews.Select(sv => sv.Item1))
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
