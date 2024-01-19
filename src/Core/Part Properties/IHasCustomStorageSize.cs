using Parts.Vehicles;

namespace Parts
{
    public interface IHasCustomStorageSize : IPart
    {
        public ICustomStorageSize StorageSizeModifier { get; set; }
    }
}
