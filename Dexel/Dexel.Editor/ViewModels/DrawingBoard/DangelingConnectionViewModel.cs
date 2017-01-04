using System;
using System.Collections.Generic;
using System.Windows.Media;
using Dexel.Editor.Views;
using Dexel.Editor.Views.CustomControls;
using Dexel.Editor.Views.DragAndDrop;
using Dexel.Library;
using Dexel.Model.DataTypes;
using PropertyChanged;
using Roslyn.Validator;

namespace Dexel.Editor.ViewModels.DrawingBoard
{
    public enum ValidationFlag
    {
        Warning,
        Invalid,
        Valid
    }

    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable, IDropable, IInputOutputViewModel
    {
        public DangelingConnectionViewModel()
        {
            ValidationFlag = ValidationFlag.Valid;
            DataNames = "param";
        }

        public Guid ID;
        public DataStreamDefinition Model { get; set; }
        public ValidationFlag ValidationFlag { get; set; }
        public string ValidationErrorMessage { get; set; }
        public FunctionUnit Parent { get; set; }
        public string DataNames { get; set; }
        public string Actionname { get; set; }
        public double Width { get; set; }



        Type IDragable.DataType => typeof (DangelingConnectionViewModel);



        public void LoadFromModel(FunctionUnit parent, DataStreamDefinition dataStream)
        {
            ID = dataStream.ID;
            Model = dataStream;
            Parent = parent;
            DataNames = dataStream.DataNames;
            Actionname = dataStream.ActionName;
        }


        public static void LoadFromModel(DangelingConnectionViewModel vm, FunctionUnit parent,
            DataStreamDefinition dataStream)
        {
            vm.LoadFromModel(parent, dataStream);
        }

        public List<Type> AllowedDropTypes => new List<Type> { typeof(DangelingConnectionViewModel), typeof(ConnectionAdapterViewModel), typeof(ConnectionViewModel) };
       


        public void Drop(object data)
        {
            data.TryCast<DangelingConnectionViewModel>(
                dangConnVm => Interactions.DragDroppedTwoDangelingConnections(dangConnVm.Model, Model, MainViewModel.Instance().Model));
            data.TryCast<ConnectionAdapterViewModel>(
               dangConnVm => Interactions.SwapDataStreamOrder(dangConnVm.Model, Model, MainViewModel.Instance().Model));
            data.TryCast<ConnectionViewModel>(
               connVm => Interactions.ChangeConnectionDestination(connVm.Model, Model.Parent, MainViewModel.Instance().Model));
        }


        public void SetValidationError(ValidationError error, string msg)
        {
            if (error.TypeOfError == TypeOfError.Error)
                ValidationFlag = ValidationFlag.Invalid;
            if (error.TypeOfError == TypeOfError.Warning)
                if (ValidationFlag != ValidationFlag.Invalid)
                    ValidationFlag = ValidationFlag.Warning;

            ValidationErrorMessage += msg;
        }
    }
}