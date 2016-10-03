using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SoftwareCell = SharpFlowDesign.Model.SoftwareCell;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class IOCellViewModel : IDropable
    {
        public IOCellViewModel()
        {
            DangelingInputs = new ObservableCollection<DangelingConnectionViewModel>();
            DangelingOutputs = new ObservableCollection<DangelingConnectionViewModel>();

        }

        public string Name { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingInputs { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingOutputs { get; set; }
        public Point Position { get; set; }
        public bool IsSelected { get; set; }
//        public List<ConnectionArrow> ArrowLinesStart { get; set; }
//        public List<ConnectionArrow> ArrowLinesEnd { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        public Point InputPoint { get; set; }
        public Point OutputPoint { get; set; }


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


        public static IOCellViewModel Create(SoftwareCell cell)
        {
            var newIOCellViewModel =  new IOCellViewModel();
            newIOCellViewModel.Name = cell.Name;
            cell.InputStreams.ToList().ForEach(stream =>
            {
                if (stream.Sources.Count != 0) return;
                newIOCellViewModel.DangelingInputs.Add(new DangelingConnectionViewModel(newIOCellViewModel)
                {
                    Datanames = stream.DataNames,
                    Actionname = stream.ActionName
                });
            });

            cell.OutputStreams.ToList().ForEach(stream =>
            {
                if (stream.Destinations.Count != 0) return;
                newIOCellViewModel.DangelingOutputs.Add(new DangelingConnectionViewModel(newIOCellViewModel)
                {
                    Datanames = stream.DataNames,
                    Actionname = stream.ActionName
                });
            });


            return newIOCellViewModel;
            
        }


        public void RemoveDangelingConnection(DangelingConnectionViewModel dangelingConnectionViewModel)
        {
            DangelingInputs.Remove(dangelingConnectionViewModel);
            DangelingOutputs.Remove(dangelingConnectionViewModel);
        }


        public Type DataType => typeof(ConnectionViewModel);

        public void Drop(object data, int index = -1)
        {
            //if moving within organization, reassign the children to the 
            //level above first
            DangelingConnectionViewModel dangelingConnection = data as DangelingConnectionViewModel;
            
//            ElementViewModel elem = data as ElementViewModel;

            if (dangelingConnection != null)
            {
                //org.isMoveWithinOrganization = true;
                //                if (org.ID == this.ID) //if dragged and dropped yourself, don't need to do anything
                //                    return;
                MainViewModel.Instance().Connections.Add(
                    new ConnectionViewModel(dangelingConnection.IOCellViewModel, this)
                    {
                        Name = dangelingConnection.Datanames
                    });
                
            }
//           this.Children = this.GetChildren();  //refresh view
        }
    }
}
