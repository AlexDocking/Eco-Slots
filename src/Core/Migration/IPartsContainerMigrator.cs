using Eco.Gameplay.Objects;
using Eco.Shared.Localization;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Parts.Migration
{
    public interface IPartsContainerMigrator
    {
        IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer);
    }
}
