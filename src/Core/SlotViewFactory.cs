namespace Parts.UI
{
    /// <summary>
    /// Used by <see cref="SlotsUIComponent"/> to create a view/controller for each slot.
    /// A mod could replace the autogen views used to display a slot should they wish, if, for instance, they define their own type of slot.
    /// </summary>
    public class SlotViewFactory
    {
        public virtual object CreateView(ISlot slot)
        {
            switch (slot)
            {
                case InventorySlot inventorySlot: return new InventorySlotController(inventorySlot);
                default: return null;
            }
        }
    }
}
