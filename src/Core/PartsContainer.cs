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
    public interface IPartsContainer : IController, INotifyPropertyChanged
    {
        IReadOnlyList<IPart> Parts { get; }
        IReadOnlyList<ISlot> Slots { get; }
        IPartsContainerSlotRestrictionManager SlotRestrictionManager { get; set; }
        ThreadSafeAction<ISlot> NewPartInSlotEvent { get; }

        bool TryAddSlot(ISlot slot, IPart part);
        void Initialize(WorldObject worldObject);
        void RemovePart(ISlot slot);
    }
    [Serialized]
    public class PartsContainer : IPartsContainer, IClearRequestHandler
    {
        //not used
        [Serialized] public string Name { get; set; } = "Serialized Name";
        public IReadOnlyList<IPart> Parts => Slots.SelectNonNull(slot => slot.Part).ToList();
        [Serialized]
        private ThreadSafeList<ISlot> slots = new ThreadSafeList<ISlot> ();
        public IReadOnlyList<ISlot> Slots => slots.Snapshot.AsReadOnly();
        public IPartsContainerSlotRestrictionManager SlotRestrictionManager { get; set; }

        /// <summary>
        /// Called when a slot gains, loses or gets a different part
        /// </summary>
        [Notify] public ThreadSafeAction<ISlot> NewPartInSlotEvent { get; } = new ThreadSafeAction<ISlot> ();
        /// <summary>
        /// Called when any part changes and property e.g. colour, or when any slot gains, loses or gets a different part
        /// </summary>
        public static ThreadSafeAction<IPartsContainer> PartsContainerChangedEventGlobal { get; } = new ThreadSafeAction<IPartsContainer>();
        public bool TryAddSlot(ISlot slot, IPart part)
        {
            if (part != null && !slot.TryAddPart(part)) return false;
            slots.Add(slot);
            return true;
        }
        public void RemovePart(ISlot slot)
        {
            int index = slots.IndexOf(slot);
            if (index >= 0)
            {
                slots.RemoveAt(index);
            }
        }

        public void Initialize(WorldObject worldObject)
        {
            foreach (ISlot slot in Slots)
            {
                slot.Initialize(worldObject, this);
                slot.NewPartInSlotEvent.Add(() => OnSlotChangedPart(slot));
                slot.PartPropertyChangedEvent.Add((_, _, _) => PartsContainerChangedEventGlobal.Invoke(this));
            }
        }

        private void OnSlotChangedPart(ISlot slot)
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
