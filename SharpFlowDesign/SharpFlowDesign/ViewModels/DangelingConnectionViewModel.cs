using System;
using PropertyChanged;
using SharpFlowDesign.Behavior;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable
    {
        public DangelingConnectionViewModel(IOCellViewModel ioCellViewModel)
        {
            Datanames = "Parameter";
            Actionname = "";

            IOCellViewModel = ioCellViewModel;
        }

        public string Datanames { get; set; }
        public string Actionname { get; set; }
        public IOCellViewModel IOCellViewModel { get; set; }

        Type IDragable.DataType => typeof (ConnectionViewModel);

        void IDragable.Remove(object i)
        {
            IOCellViewModel.RemoveDangelingConnection(this);
        }
    }
}