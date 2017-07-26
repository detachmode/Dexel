using System;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [Serializable]
    [ImplementPropertyChanged]
    public class SketchRectangle
    {
        public Guid Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public string Name { get; set; }
        public string Interaction { get; set; }
    }
}
