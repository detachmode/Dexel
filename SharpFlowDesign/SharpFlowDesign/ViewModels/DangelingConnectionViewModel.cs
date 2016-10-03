using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using SharpFlowDesign.Behavior;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable
    {
        public string Datanames { get; set; }
        public string Actionname { get; set; }
        public IOCellViewModel IOCellViewModel { get; set; }

        public DangelingConnectionViewModel(IOCellViewModel ioCellViewModel)
        {
            Datanames = "Parameter";
            Actionname = "";

            IOCellViewModel = ioCellViewModel;
        }

        Type IDragable.DataType => typeof(ConnectionViewModel);

        void IDragable.Remove(object i)
        {
           IOCellViewModel.RemoveDangelingConnection(this);
        }
    }
}
