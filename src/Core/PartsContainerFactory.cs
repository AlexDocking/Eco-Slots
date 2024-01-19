using Eco.Shared.Localization;
using Eco.Shared.Utils;

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
