namespace Parts
{
    /// <summary>
    /// A part which should increase or decrease the number of storage slots the world object has.
    /// </summary>
    public interface IPartWithCustomStorageSize : IPart
    {
        public ICustomStorageSize StorageSizeModifier { get; set; }
    }
}
