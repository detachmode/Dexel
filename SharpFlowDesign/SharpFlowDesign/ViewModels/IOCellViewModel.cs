using System.Windows;
using PropertyChanged;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class IOCellViewModel
    {

        public string Name { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public Point Position { get; set; }
        public bool IsSelected { get; set; }



        public void Move(double x, double y)
        {
            var pos = this.Position;
            pos.X += x;
            pos.Y += y;
            this.Position = pos;
        }



        public void Deselect()
        {
            this.IsSelected = false;
        }


        public void Select()
        {
            this.IsSelected = true;
        }
    }
}
