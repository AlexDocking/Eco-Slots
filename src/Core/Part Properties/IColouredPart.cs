namespace Parts
{
    /// <summary>
    /// A part which has a colour. It could be used for changing the colour of a lightbulb, or the colour of a table for instance.
    /// </summary>
    public interface IColouredPart : IPart
    {
        public ModelPartColourData ColourData { get; }
    }
}
