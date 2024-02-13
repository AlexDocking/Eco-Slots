namespace Parts
{
    public static class PartsContainerFactory
    {
        /// <summary>
        /// Create a new empty parts container. New slots can be added before it is initialised.
        /// </summary>
        /// <returns></returns>
        public static IPartsContainer Create()
        {
            IPartsContainer partsContainer = new PartsContainer();
            return partsContainer;
        }
        /// <summary>
        /// Create a new parts container, populating it with new slots based on the slot definitions in the schema.
        /// </summary>
        public static IPartsContainer Create(IPartsContainerSchema schema)
        {
            IPartsContainer partsContainer = new PartsContainer();
            for (int i = 0; i < schema.SlotDefinitions.Count; i++)
            {
                partsContainer.TryAddSlot(schema.SlotDefinitions[i].MakeSlotFromDefinition(), null);
            }
            return partsContainer;
        }
    }
}
