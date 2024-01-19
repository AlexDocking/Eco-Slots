namespace Parts
{
    public interface IHasModelPartColour : IPart
    {
        public ModelPartColouring ColourData { get; }
    }
}
