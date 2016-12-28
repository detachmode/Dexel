using Dexel.Model.DataTypes;
using Roslyn.Validator;

namespace Dexel.Editor.ViewModels.DrawingBoard
{
    public interface IInputOutputViewModel
    {
        DataStreamDefinition Model { get; set; }
        bool IsInvalid { get; set; }
        string ValidationErrorMessage { get; set; }
        void SetToInvalid(ValidationErrorUnnconnectedOutput error );
    }
}
