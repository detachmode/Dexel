using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dexel.Model.DataTypes
{
    public class SketchRectangle
    {
        public Guid Id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
    }
}
