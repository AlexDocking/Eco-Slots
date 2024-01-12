using Eco.Shared.Serialization;

namespace Parts
{
    [Serialized]
    public interface IPart
    {
        public string DisplayName { get; }
    }
}
