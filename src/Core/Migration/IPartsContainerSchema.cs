using Eco.Gameplay.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts.Migration
{
    public interface IPartsContainerSchema
    {
        IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer);
    }
    public class KitchenBaseCabinetSchema : IPartsContainerSchema
    {
        public IPartsContainer Migrate(WorldObject worldObject, IPartsContainer existingContainer)
        {
            return null;
        }
    }
}
