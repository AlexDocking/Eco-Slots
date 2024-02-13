using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    /// <summary>
    /// WIP. This feature needs expanding so that slots of all types have statuses.
    /// </summary>
    public interface ISlotStatus
    {
        bool Violated { get; }
    }
    /// <summary>
    /// WIP. Used for checking whether a part can be removed when the WorldObject's storage is not empty.
    /// </summary>
    public class RequireEmptyStorageSlotStatus : ISlotStatus
    {
        public RequireEmptyStorageSlotStatus(Inventory storage)
        {
            Storage = storage;
            previouslyEmpty = Storage.IsEmpty;
            Storage.OnChanged.Add(OnInventoryChanged);
        }
        public bool Violated => !Storage.IsEmpty;
        public ThreadSafeAction StatusChangedEvent { get; } = new ThreadSafeAction();
        private Inventory Storage { get; }
        private bool previouslyEmpty;
        private void OnInventoryChanged(User user)
        {
            bool nowEmpty = Storage.IsEmpty;
            if (nowEmpty != previouslyEmpty)
            {
                StatusChangedEvent.Invoke();
            }
            previouslyEmpty = Storage.IsEmpty;
        }
    }
}
