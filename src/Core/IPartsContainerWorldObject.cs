using Parts.Migration;

namespace Parts
{
    public interface IPartsContainerWorldObject
    {
        public IPartsContainerMigrator GetPartsContainerMigrator();
    }
}
