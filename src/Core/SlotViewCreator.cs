namespace Parts
{
    public class SlotViewCreator
    {
        public virtual object CreateView(ISlot slot)
        {
            switch (slot)
            {
                case InventorySlot inventorySlot: return new SlotViewController(inventorySlot);
                default: return null;
            }
        }
    }
}
