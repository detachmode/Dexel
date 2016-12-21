using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dexel.Model.DataTypes;

namespace Dexel.Model.Manager
{

    public static class FunctionUnitManager
    {
        public static IEnumerable<FunctionUnit> GetAll(Guid functionUnitID, MainModel mainModel)
        {
            return mainModel.FunctionUnits.Where(x => x.ID.Equals(functionUnitID));
        }


        public static FunctionUnit GetFirst(Guid destinationID, MainModel mainModel)
        {
            return mainModel.FunctionUnits.First(x => x.ID.Equals(destinationID));
        }

        public static void IsIntegration(this FunctionUnit functionUnit, Action isIntegration, Action isNotIntegration )
        {
            if (functionUnit.IsIntegrating.Any())
                isIntegration();
            else
                isNotIntegration();
        }

        public static void MoveX(this FunctionUnit functionUnit, double offsetx)
        {
            var pt = functionUnit.Position;
            pt.X += offsetx;
            functionUnit.Position = pt;
        }

        public static void MoveY(this FunctionUnit functionUnit, double offsety)
        {
            var pt = functionUnit.Position;
            pt.Y += offsety;
            functionUnit.Position = pt;
        }

        public static void MovePosition(this FunctionUnit functionUnit, Vector dragDelta)
        {
            var pt = functionUnit.Position;
            pt.X += dragDelta.X;
            pt.Y += dragDelta.Y;
            functionUnit.Position = pt;
        }

        public static FunctionUnit GetFristByID(Guid destinationID, MainModel mainModel)
        {
            return mainModel.FunctionUnits.First(x => x.ID.Equals(destinationID));
        }


        public static FunctionUnit CreateNew(string name = "")
        {
            return new FunctionUnit
            {
                ID = Guid.NewGuid(),
                Name = name
            };
        }


        public static void RemoveDefinitionsFromSourceAndDestination(DataStreamDefinition defintion, FunctionUnit source,
            FunctionUnit destination)
        {
            source.OutputStreams.RemoveAll(
                x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
            destination.InputStreams.RemoveAll(
                x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
        }


        public static DataStreamDefinition NewInputDef(FunctionUnit functionUnit, string datanames, string actionName)
        {
            var definition = DataStreamManager.NewDefinition(functionUnit, datanames, actionName);
            functionUnit.InputStreams.Add(definition);
            return definition;
        }


        public static DataStreamDefinition NewOutputDef(FunctionUnit functionUnit, string datanames, string actionName)
        {
            var definition = DataStreamManager.NewDefinition(functionUnit, datanames, actionName);
            functionUnit.OutputStreams.Add(definition);
            return definition;
        }
    }

}