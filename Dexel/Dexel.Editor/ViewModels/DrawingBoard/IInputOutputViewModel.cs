using Dexel.Model.DataTypes;
using Roslyn.Validator;

namespace Dexel.Editor.ViewModels.DrawingBoard
{
    public interface IInputOutputViewModel
    {
        DataStreamDefinition Model { get; set; }
        ValidationFlag ValidationFlag { get; set; }
        string ValidationErrorMessage { get; set; }
        void SetValidationError(ValidationError error, string msg);
    }
}
