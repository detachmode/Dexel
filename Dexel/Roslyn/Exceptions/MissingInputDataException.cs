using System;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Roslyn
{
    internal class MissingInputDataException : Exception
    {
        public override string Message { get; } = "";

        public MissingInputDataException(DataStreamDefinition dsd, NameType needed)
        {
            Message = "Couldn't find matching type in flow for input of: \n" +
                    $"\tfunction unit:\t{dsd.Parent.Name}\n\n" +
                    "\tNeeded Type that was not found:\n" +
                    $"\tname:\t\t{needed.Name}\n" +
                    $"\ttype:\t\t{needed.Type}\n" +
                    $"\tIsArray:\t{needed.IsArray}\n" +
                    $"\tIsCollection:\t{needed.IsList}\n";
        }
    }

}