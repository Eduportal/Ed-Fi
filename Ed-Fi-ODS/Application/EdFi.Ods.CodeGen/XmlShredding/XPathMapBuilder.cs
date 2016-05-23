namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    using Newtonsoft.Json;

    /// <summary>
    /// Reads XSD and produces an acyclic graph of objects that represent potential XML.
    /// 
    /// Can read a serialized JSON representation of an acyclic graph and produce the actual graph
    /// </summary>
    public partial class XPathMapBuilder : IXPathMapBuilder
    {
        private RawMap _map;

        public IMapStep DeserializeMap(string serializedMap)
        {
            var termStep = JsonConvert.DeserializeObject<RawStep>(serializedMap);
            var stepToTerminalElement = new ElementStep(new[] { termStep.ParentElement, termStep.ElementName }, null);
            if (termStep.IsParentIdentity)
                stepToTerminalElement = new ElementStep(new[] { termStep.ElementName }, null);
            return termStep.HasParent() ? BuildNextStepUntilReachesBase(stepToTerminalElement, termStep.Parent) : stepToTerminalElement;
        }

        private IMapStep BuildNextStepUntilReachesBase(IMapStep childStep, RawStep nextStep)
        {
            var elements = new List<string>();
            if (!string.IsNullOrWhiteSpace(nextStep.ParentElement) && !nextStep.IsParentIdentity)
                elements.Add(nextStep.ParentElement);
            elements.Add(nextStep.ElementName);
            IMapStep step;
            if (nextStep.IsReference)
            {
                var referenceDictionary = new Dictionary<string, IMapStep>();
                if (nextStep.IsEdOrgRef)
                {
                    referenceDictionary = new Dictionary<string, IMapStep>(EdOrgReferenceDictionary);
                }
                //We still might have a reference to another reference element
                referenceDictionary.Add(nextStep.ReferenceTarget, childStep);
                referenceDictionary.Add(nextStep.ElementName, childStep);
                step = new ReferenceStep(elements.ToArray(), childStep, referenceDictionary);
            }
            else
            {
                step = new ElementStep(elements.ToArray(), childStep);
            }
            if (!nextStep.HasParent()) return step;
            return this.BuildNextStepUntilReachesBase(step, nextStep.Parent);
        }

        public IEnumerable<Tuple<ParsedSchemaObject, string>> BuildStartingXsdElementSerializedMapTuplesFor(ParsedSchemaObject parentXsd)
        {
            if (parentXsd.IsExtendedReference() || parentXsd.IsExtendedReferenceCollection())
            {
                this.ResetMap();
                return this.RecurseReference(parentXsd);
            }
            if (!parentXsd.ChildElements.Any(c => c.ContainsForeignKey() && !c.IsCollection))
                return new List<Tuple<ParsedSchemaObject, string>>();
            var keyObjectContainers = parentXsd.ChildElements.Where(c => c.ContainsForeignKey() && !c.IsCollection).ToList();
            var nestedContainers = parentXsd.ChildElements.Where(c => c.IsCommonExpansion()).ToList();
            foreach (var schemaObject in nestedContainers)
            {
                keyObjectContainers.AddRange(schemaObject.ChildElements.Where(c => c.ContainsForeignKey() && !c.IsCollection).ToArray());
            }
            var tuples = new List<Tuple<ParsedSchemaObject, string>>();
            foreach (var schemaObject in keyObjectContainers.Where(c => c.IsExtendedReference()))
            {
                this.ResetMap();
                tuples.AddRange(this.RecurseReference(schemaObject));
            }
            foreach (var identity in keyObjectContainers.Where(c => c.IsIdentity()))
            {
                this.ResetMap();
                tuples.AddRange(this.RecurseOnIdentity(identity));
            }
            this.ResetMap();
            tuples.AddRange(keyObjectContainers.Where(c => c.IsDescriptorsExtRef() && c.ChildElements.Any(ce => ce.IsTerminalProperty()))
                .Select(dref => new Tuple<ParsedSchemaObject, string>(dref.ChildElements.Single(e => e.IsTerminalProperty()), this.BuildMapTo(dref, true))));
            return tuples;
        }

        private void ResetMap()
        {
            this._map = new RawMap();
        }

        private IEnumerable<Tuple<ParsedSchemaObject, string>> RecurseReference(ParsedSchemaObject obj)
        {
            this._map.MoveMapTo(obj.ParentElement.XmlSchemaObjectName.Contains("Identity")
                ? obj.ParentElement.ParentElement.XmlSchemaObjectName
                : obj.ParentElement.XmlSchemaObjectName);

            this.AddReferenceStepAndMakeActive(obj);
            //We are going to assume Reference's always have an Identity group
            var idObj = obj.ChildElements.Single(c => c.IsIdentity());

            return this.RecurseOnIdentity(idObj);
        }

        private IEnumerable<Tuple<ParsedSchemaObject, string>> RecurseOnIdentity(ParsedSchemaObject identity)
        {
            var propertyTuples = new List<Tuple<ParsedSchemaObject, string>>();

            propertyTuples.AddRange(identity.ChildElements.Where(c => c.IsTerminalProperty())
                .Select(terminalXsd => new Tuple<ParsedSchemaObject, string>(terminalXsd, this.BuildMapTo(terminalXsd))));
            propertyTuples.AddRange(identity.ChildElements.Where(c => c.IsDescriptorsExtRef() && c.ChildElements.Any(ce => ce.IsTerminalProperty()))
                .Select(descriptorsRef => new Tuple<ParsedSchemaObject, string>(descriptorsRef.ChildElements.Single(c => c.IsTerminalProperty()), this.BuildMapTo(descriptorsRef, true))));

            foreach (var extendedRef in identity.ChildElements.Where(c => c.IsExtendedReference()))
            {
                propertyTuples.AddRange(this.RecurseReference(extendedRef));
            }
            return propertyTuples;
        }

        private string BuildMapTo(ParsedSchemaObject terminalObject, bool isDescriptorRef = false)
        {
            RawStep termStep;
            if (isDescriptorRef)
            {
                termStep = new RawStep
                {
                    Parent = this._map.ActiveStep,
                    ParentElement = terminalObject.XmlSchemaObjectName,
                    IsReference = true,
                    ElementName = terminalObject.ChildElements.Single(c => c.IsTerminalProperty()).XmlSchemaObjectName,
                };
            }
            else
            {
                termStep = new RawStep { Parent = this._map.ActiveStep, ElementName = terminalObject.XmlSchemaObjectName };
                if (terminalObject.ParentElement.IsIdentity())
                {
                    termStep.IsParentIdentity = true;
                    termStep.ParentElement = terminalObject.ParentElement.XmlSchemaObjectName;
                }
            }

            return JsonConvert.SerializeObject(termStep);
        }

        private void AddReferenceStepAndMakeActive(ParsedSchemaObject refObject)
        {
            if (this._map.ActiveStep != null && this._map.ActiveStep.ElementName.Equals(refObject.XmlSchemaObjectName)) return;

            var refStep = new RawStep { Parent = this._map.ActiveStep, ElementName = refObject.XmlSchemaObjectName, IsReference = true };
            if (refObject.IsEdOrgReference()) refStep.IsEdOrgRef = true;
            if (refObject.ParentElement.IsIdentity())
            {
                refStep.ParentElement = refObject.ParentElement.XmlSchemaObjectName;
                refStep.IsParentIdentity = true;
            }
            if (refObject.ChildElements.Any(c => c.IsReference()))
            {
                refStep.ReferenceTarget =
                    ((ExpectedReferencedElement)
                        refObject.ChildElements.Single(c => c.IsReference()).ProcessResult.Expected)
                        .ReferencedElementName;
            }
            this._map.AddStep(refStep);
            this._map.MoveMapTo(refStep);
        }

        internal class RawMap
        {
            private List<RawStep> _steps;

            public RawMap()
            {
                this._steps = new List<RawStep>();
                this.ActiveStep = null;
            }

            public RawStep ActiveStep { get; private set; }

            public void MoveMapTo(string elementName, string parentElementName = null)
            {
                if (this._steps.Any(s => s.ElementName.Equals(elementName)))
                    this.ActiveStep = this._steps.First(s => s.ElementName.Equals(elementName));
            }

            public void MoveMapTo(RawStep newActiveStep)
            {
                if (this._steps.Contains(newActiveStep)) this.ActiveStep = newActiveStep;
            }

            public void AddStep(RawStep step)
            {
                this._steps.Add(step);
            }
        }

        /// <summary>
        /// Stores information about potential XML, and provides a way to persist that information in a serialized format.
        /// 
        /// The graph is actually a tree because the links are only from child to parent
        /// 
        /// The graph is built from the innermost element up to the parent.  This allows us to traverse the XML from the 
        /// outside in later.  So, to read the XSD, we have to read from the outside into the terminal element, then persist
        /// the graph as we unroll the recursion.
        /// </summary>
        internal class RawStep
        {
            public bool IsParentIdentity { get; set; }
            public bool IsReference { get; set; }
            public string ReferenceTarget { get; set; }
            public RawStep Parent { get; set; }
            public string ElementName { get; set; }
            public string ParentElement { get; set; }
            public bool IsEdOrgRef { get; set; }

            public bool HasParent()
            {
                return this.Parent != null;
            }
        }
    }
}