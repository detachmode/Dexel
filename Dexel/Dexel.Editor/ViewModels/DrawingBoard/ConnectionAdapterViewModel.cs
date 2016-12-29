using System;
using System.Collections.Generic;
using Dexel.Editor.Views;
using Dexel.Editor.Views.DragAndDrop;
using Dexel.Library;
using Dexel.Model.DataTypes;
using PropertyChanged;
using Roslyn.Validator;

namespace Dexel.Editor.ViewModels.DrawingBoard
{
    [ImplementPropertyChanged]
    public class ConnectionAdapterViewModel : IDragable, IDropable, IInputOutputViewModel
    {
        public Guid ID;
        public FunctionUnit Parent { get; set; }
        public string Actionname { get; set; }
        public double Width { get; set; }
        public ValidationFlag ValidationFlag { get; set; }
        public string ValidationErrorMessage { get; set; }
        public DataStreamDefinition Model { get; set; }

        Type IDragable.DataType => typeof(ConnectionAdapterViewModel);

        public List<Type> AllowedDropTypes
            => new List<Type> {typeof(ConnectionAdapterViewModel), typeof(DangelingConnectionViewModel)};


        public void Drop(object data)
        {
            data.TryCast<ConnectionAdapterViewModel>(
                droppedData =>
                        Interactions.SwapDataStreamOrder(droppedData.Model, Model, MainViewModel.Instance().Model));
            data.TryCast<DangelingConnectionViewModel>(
                droppedData =>
                        Interactions.SwapDataStreamOrder(droppedData.Model, Model, MainViewModel.Instance().Model));
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


        public void LoadFromModel(FunctionUnit parent, DataStreamDefinition dataStream)
        {
            ID = dataStream.ID;
            Model = dataStream;
            Parent = parent;
            Actionname = dataStream.ActionName;
        }


        public static void LoadFromModel(ConnectionAdapterViewModel vm, FunctionUnit parent,
            DataStreamDefinition dataStream)
        {
            vm.LoadFromModel(parent, dataStream);
        }
    }
}