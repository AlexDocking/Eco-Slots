using Eco.Core.Controller;
using Eco.Core.PropertyHandling;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parts
{
    [Serialized]
    public class PartsContainer : IController, INotifyPropertyChanged, IClearRequestHandler
    {
        [Serialized] public string Name { get; set; } = "Serialized Name";
        //[SyncToView, NewTooltipChildren(Eco.Shared.Items.CacheAs.Instance)]
        public IReadOnlyList<IPart> Parts => Slots.SelectNonNull(slot => slot.Inventory.Stacks.FirstOrDefault()?.Item).OfType<IPart>().ToList();
        [Serialized]
        private ThreadSafeList<Slot> slots = new ThreadSafeList<Slot> ();
        public IReadOnlyList<Slot> Slots => slots.Snapshot.AsReadOnly();
        public ISlotRestrictionManager SlotRestrictionManager { get; set; }

        /// <summary>
        /// Called when a slot gains, loses or gets a different part
        /// </summary>
        [Notify] public ThreadSafeAction<Slot> NewPartInSlotEvent { get; } = new ThreadSafeAction<Slot> ();
        /// <summary>
        /// Called when any part changes and property e.g. colour, or when any slot gains, loses or gets a different part
        /// </summary>
        public static ThreadSafeAction<PartsContainer> PartsContainerChangedEventGlobal { get; } = new ThreadSafeAction<PartsContainer>();
        public void AddPart(Slot slot, IPart part)
        {
            slot.TryAddPart(part);
            slots.Add(slot);
        }
        public void RemovePart(Slot slot)
        {
            int index = slots.IndexOf(slot);
            if (index >= 0)
            {
                slots.RemoveAt(index);
            }
        }

        public void Initialize(WorldObject worldObject)
        {
            Log.WriteLine(Localizer.DoStr($"Initialize parts container {Name} with object {worldObject?.Name}"));

            foreach (Slot slot in Slots)
            {
                slot.Initialize(worldObject, this);
                slot.NewPartInSlotEvent.Add(() => OnSlotChangedPart(slot));
                slot.PartPropertyChangedEvent.Add((_, _, _) => PartsContainerChangedEventGlobal.Invoke(this));
            }
        }

        private void OnSlotChangedPart(Slot slot)
        {
            NewPartInSlotEvent.Invoke(slot);
            PartsContainerChangedEventGlobal.Invoke(this);
        }

        public Result TryHandleClearRequest(Player player)
        {
            return Result.Fail(Localizer.DoStr("Cannot delete slots"));
        }
        #region IController
        private int id;

        public ref int ControllerID => ref id;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
