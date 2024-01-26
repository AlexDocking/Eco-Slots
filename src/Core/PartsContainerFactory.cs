namespace Parts
{
    public static class PartsContainerFactory
    {
        public static IPartsContainerFactory Factory { get; set; } = new DefaultPartsContainerFactory();
        public static IPartsContainer Create()
        {
            IPartsContainer partsContainer = Factory.Create();

            return partsContainer;
        }
        public static IPartsContainer Create(IPartsContainerSchema schema)
        {
            IPartsContainer partsContainer = Factory.Create();
            for (int i = 0; i < schema.SlotSchemas.Count; i++)
            {
                partsContainer.TryAddSlot(new InventorySlot(schema.SlotSchemas[i]), null);
            }
            return partsContainer;
        }
    }
    public interface IPartsContainerFactory
    {
        IPartsContainer Create();
    }
    public class DefaultPartsContainerFactory : IPartsContainerFactory
    {
        public IPartsContainer Create() => new PartsContainer();
    }
}
