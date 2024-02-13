using Eco.Gameplay.Objects;

namespace Parts.Migration
{
    /// <summary>
    /// WIP.
    /// Migrate an existing container's parts should the schema change or a new and incompatible version of the <see cref="PartsContainer"/> class is available.
    /// Since mods cannot use Eco's migration unless the game version increases,
    /// breaking updates to the PartsContainer or slot classes may have to involve new and separate versions of those classes e.g. PartsContainerV2.
    /// Those parts would then need to be placed in updated versions of slots, and in updated versions of the PartsContainer.
    /// </summary>
    public interface IPartsContainerMigrator
    {
        IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer);
    }
}
