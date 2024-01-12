namespace Parts
{
    public interface IHasModelPartColourComponent : IPart
    {
        public ModelPartColouring ColourData { get; }
    }
}
