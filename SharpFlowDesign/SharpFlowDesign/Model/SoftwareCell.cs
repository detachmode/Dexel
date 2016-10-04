using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PropertyChanged;

namespace SharpFlowDesign.Model
{

    [ImplementPropertyChanged]
    public partial class SoftwareCell
    {
        public string Name { get; set; }
        public FlowAttribute Attribute { get; set; } // Pyramide or Barrel

        public SoftwareCell Integration { get; private set; } // ZOOM into this Node. Start node of the integrated flow

        // Inputs / Outputs
        private readonly List<DangelingConnection> _dangelingInputs = new List<DangelingConnection>();
        public ReadOnlyCollection<DangelingConnection> DangelingInputs => _dangelingInputs.AsReadOnly();

        private readonly List<DangelingConnection> _dangelingOutputs = new List<DangelingConnection>();
        public ReadOnlyCollection<DangelingConnection> DangelingOutputs => _dangelingOutputs.AsReadOnly();


        public void AddOutput(string datanames, string actionName = null, bool optional = false)
        {
            var stream = new DangelingConnection { ActionName = actionName, DataNames = datanames};
            AddOutput(stream);
        }


        //public void AddInput(string datanames, string actionName = null, bool optional = false)
        //{
        //    var stream = new Stream {ActionName = actionName, DataNames = datanames, Optional = optional};
        //    AddInput(stream);
        //}


        //public void SetIntegration(ConnectedToCell integration)
        //{
        //    var foundMatchingInput = InputConnections.Any(x =>
        //        integration.InputConnections.Any(y => x.DataNames.Equals(y.DataNames)));
        //    if (!foundMatchingInput)
        //        throw new Exception("Integration inputs are not matching with inputs of integrated StreamViewModel!");

        //    Integration = integration;
        //}


        //public void CheckForStreamWithSameName(ConnectedToCell destination, Stream tempStream,
        //    Action<Stream, ConnectedToCell> onFound, Action<ConnectedToCell, Stream> onNotFound)
        //{
        //    var found =
        //        OutputConnections.Where(
        //            x => x.DataNames.Equals(tempStream.DataNames) && x.Optional.Equals(tempStream.Optional)).ToList();
        //    if (found.Any())
        //        onFound(found.First(), destination);
        //    else
        //        onNotFound(destination, tempStream);
        //}


        //public void AddToExistingConnection(Stream foundStream, ConnectedToCell destination)
        //{
        //    foundStream.AddSource(this);
        //    foundStream.AddDestination(destination);
        //}


        //public void AddNewConnection(ConnectedToCell destination, Stream stream)
        //{
        //    AddOutput(stream);
        //    destination.AddInput(stream);
        //}


        private void AddOutput(DangelingConnection dangelingConnection)
        {
            dangelingConnection.IOCellViewModel.AddSource(this);
            _dangelingOutputs.Add(stream);
        }


        //private void AddInput(Stream stream)
        //{
        //    stream.AddDestination(this);
        //    _dangelingInputs.Add(stream);
        //}


        //private void RemoveStream(Stream stream)
        //{
        //    _dangelingInputs.RemoveAll(x => x == stream);
        //    _dangelingOutputs.RemoveAll(x => x == stream);
        //}


        public void RemoveDangelingConnection(DangelingConnection dangelingConnection)
        {
            throw new NotImplementedException();
        }
    }


    public enum FlowAttributeType
    {
        Provider,
        State
    }


    public class FlowAttribute
    {
        public string Name { get; set; }
        public FlowAttributeType Type { get; set; }
    }

}