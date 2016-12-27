using System;
using Dexel.Model.DataTypes;

namespace Roslyn.Exceptions
{
    public class UnnconnectedOutputException : Exception
    {
        public override string Message { get; } = "";

        public UnnconnectedOutputException(DataStreamDefinition dsd)
        {
            Message = "Unconnected output found that is not output of integration: \n" +
                      $"\tfunction unit:\t{dsd.Parent.Name}\n" +
                      $"\tdata names:\t{dsd.DataNames}\n" +
                      $"\taction name:\t{dsd.ActionName}\n";
        }
    }
}