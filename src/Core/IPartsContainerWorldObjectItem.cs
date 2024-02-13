using Parts.Migration;

namespace Parts
{
    /// <summary>
    /// World object item that has parts.
    /// </summary>
    public interface IPartsContainerWorldObjectItem
    {
        /// <summary>
        /// Create a new parts container once the object is placed down, or migrate the existing container if it exists, 
        /// transfering existing parts to the correct slots should the schema change.
        /// </summary>
        /// <returns></returns>
        public IPartsContainerMigrator GetPartsContainerMigrator();
    }
}
