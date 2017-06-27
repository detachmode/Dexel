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
        public int Id { get; set; }
        public Point Position { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
    }
}
