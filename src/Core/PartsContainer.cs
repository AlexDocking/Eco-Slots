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
    /// <summary>
    /// Stores the list of slots on an instantiated world object.
    /// It is an interface so that it can be migrated outside of Eco's migration system, which only works when new versions of the game are released and not when the mod is updated.
    /// </summary>
    [Serialized]
    public interface IPartsContainer : IController, INotifyPropertyChanged
    {
        IReadOnlyList<IPart> Parts { get; }
        IReadOnlyList<ISlot> Slots { get; }
        ThreadSafeAction<ISlot> NewPartInSlotEvent { get; }

        bool TryAddSlot(ISlot slot, IPart part);
        void Initialize(WorldObject worldObject);
        void RemovePart(ISlot slot);
    }
    [Serialized]
    public class PartsContainer : IPartsContainer, IClearRequestHandler
    {
        public IReadOnlyList<IPart> Parts => Slots.SelectNonNull(slot => slot.Part).ToList();
        [Serialized]
        private ThreadSafeList<ISlot> slots = new ThreadSafeList<ISlot> ();
        public IReadOnlyList<ISlot> Slots => slots.Snapshot.AsReadOnly();

        /// <summary>
        /// Called when a slot gains, loses or gets a different part
        /// </summary>
        [Notify] public ThreadSafeAction<ISlot> NewPartInSlotEvent { get; } = new ThreadSafeAction<ISlot> ();
        /// <summary>
        /// Called when any part changes and property e.g. colour, or when any slot gains, loses or gets a different part. This allows the WorldObject's tooltip to be updated.
        /// </summary>
        public static ThreadSafeAction<IPartsContainer> PartsContainerChangedEventGlobal { get; } = new ThreadSafeAction<IPartsContainer>();
        public PartsContainer() { }
        public PartsContainer(IEnumerable<ISlot> slots)
        {
            this.slots.AddRange(slots);
        }
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

        /// <summary>
        /// Parent all the slots to this container and let any listeners know that this container is ready for business.
        /// </summary>
        /// <param name="worldObject"></param>
        public void Initialize(WorldObject worldObject)
        {
            foreach (ISlot slot in Slots)
            {
                slot.Initialize(worldObject, this);
                slot.NewPartInSlotEvent.Add(() => OnSlotChangedPart(slot));
                slot.PartPropertyChangedEvent.Add((_, _, _) => PartsContainerChangedEventGlobal.Invoke(this));
            }
            PartsContainerChangedEventGlobal.Invoke(this);
        }

        private void OnSlotChangedPart(ISlot slot)
        {
            NewPartInSlotEvent.Invoke(slot);
            PartsContainerChangedEventGlobal.Invoke(this);
        }

        // Don't allow players to delete the persistent data, because that would delete the installed parts!
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
