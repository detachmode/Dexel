using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dexel.Model.DataTypes;

namespace Dexel.Model.Manager
{
    public class SketchRectangleManager
    {
        private SketchRectangleManager()
        {
            _rectangles.Add(new SketchRectangle{Id = Guid.NewGuid(), Height = 900, Width = 900, Name = "Main Window", x=50,y=50});
        }

        private static SketchRectangleManager _self;
        public List<SketchRectangle> _rectangles = new List<SketchRectangle>();

        public static SketchRectangleManager Instance()
        {
            return _self ?? (_self = new SketchRectangleManager());
        }

        public SketchRectangle GetRoot()
        {
            return _rectangles[0];
        }
    }
}
