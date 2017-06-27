using System;
using System.Collections.Generic;
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
            _rectangles.Add(1, new SketchRectangle{Id = 1, Height = 300, Width = 100, Name = "Test",ParentId = 1, Position = new Point(100,100)});
            _rectangles.Add(2, new SketchRectangle { Id = 2, Height = 300, Width = 100, Name = "Drin", ParentId = 1, Position = new Point(100, 100) });
            _rectangles.Add(3, new SketchRectangle { Id = 3, Height = 300, Width = 100, Name = "HUE", ParentId = 1, Position = new Point(100, 100) });
            _rectangles.Add(4, new SketchRectangle { Id = 4, Height = 300, Width = 100, Name = "OOOLDER", ParentId = 1, Position = new Point(100, 100) });

        }

        private static SketchRectangleManager _self;
        private Dictionary<int,SketchRectangle> _rectangles = new Dictionary<int, SketchRectangle>();

        public static SketchRectangleManager Instance()
        {
            return _self ?? (_self = new SketchRectangleManager());
        }

        public SketchRectangle GetRoot()
        {
            return _rectangles[1];
        }

        public IEnumerable<SketchRectangle> GetChildren(int parentId)
        {
            return from a in _rectangles
                where a.Value.ParentId == parentId
                      && a.Value.Id != parentId   
                select a.Value;
        }

        
    }
}
