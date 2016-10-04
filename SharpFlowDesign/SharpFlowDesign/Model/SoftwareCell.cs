using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpFlowDesign.Model
{

    public partial class SoftwareCell
    {
        public string Name { get; set; }
        public FlowAttribute Attribute { get; set; } // Pyramide or Barrel

        public SoftwareCell Integration { get; private set; } // ZOOM into this Node. Start node of the integrated flow

        // Inputs / Outputs
        private readonly List<Stream> _inputStreams = new List<Stream>();
        public ReadOnlyCollection<Stream> InputStreams => _inputStreams.AsReadOnly();

        private readonly List<Stream> _outputStreams = new List<Stream>();
        public ReadOnlyCollection<Stream> OutputStreams => _outputStreams.AsReadOnly();





        public void AddOutput(string datanames, string actionName = null, bool optional = false)
        {
            var stream = new Stream {ActionName = actionName, DataNames = datanames, Optional = optional};
            AddOutput(stream);
        }


        public void AddInput(string datanames, string actionName = null, bool optional = false)
        {
            var stream = new Stream {ActionName = actionName, DataNames = datanames, Optional = optional};
            AddInput(stream);
        }


        public void SetIntegration(SoftwareCell integration)
        {
            var foundMatchingInput = InputStreams.Any(x =>
                integration.InputStreams.Any(y => x.DataNames.Equals(y.DataNames)));
            if (!foundMatchingInput)
                throw new Exception("Integration inputs are not matching with inputs of integrated StreamViewModel!");

            Integration = integration;
        }


        public void CheckForStreamWithSameName(SoftwareCell destination, Stream tempStream,
            Action<Stream, SoftwareCell> onFound, Action<SoftwareCell, Stream> onNotFound)
        {
            var found =
                OutputStreams.Where(
                    x => x.DataNames.Equals(tempStream.DataNames) && x.Optional.Equals(tempStream.Optional)).ToList();
            if (found.Any())
                onFound(found.First(), destination);
            else
                onNotFound(destination, tempStream);
        }


        public void AddToExistingConnection(Stream foundStream, SoftwareCell destination)
        {
            foundStream.AddSource(this);
            foundStream.AddDestination(destination);
        }


        public void AddNewConnection(SoftwareCell destination, Stream stream)
        {
            AddOutput(stream);
            destination.AddInput(stream);
        }


        private void AddOutput(Stream stream)
        {
            stream.AddSource(this);
            _outputStreams.Add(stream);
        }


        private void AddInput(Stream stream)
        {
            stream.AddDestination(this);
            _inputStreams.Add(stream);
        }


        private void RemoveStream(Stream stream)
        {
            _inputStreams.RemoveAll(x => x == stream);
            _outputStreams.RemoveAll(x => x == stream);
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