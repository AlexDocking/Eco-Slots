namespace Parts.Tests
{
    public static class TestUtility
    {
        public static Slot CreateSlot()
        {
            return new Slot();
        }
        public static Slot CreateSlot(ISlotDefinition slotDefinition)
        {
            return new Slot()
            {
                Name = slotDefinition.Name
            };
        }
    }
}