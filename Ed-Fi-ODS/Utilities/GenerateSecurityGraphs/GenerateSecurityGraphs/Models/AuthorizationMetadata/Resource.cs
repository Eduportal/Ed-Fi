using System.Collections.Generic;
using System.Linq;

namespace GenerateSecurityGraphs.Models.AuthorizationMetadata
{
    public class Resource
    {
        public Resource(string name)
        {
            Name = name;

            ActionAndStrategyPairs = new List<ActionAndStrategy>();
        }

        public string Name { get; set; }

        public List<ActionAndStrategy> ActionAndStrategyPairs { get; set; }

        public Resource Parent { get; set; }

        public IEnumerable<Resource> AncestorsOrSelf
        {
            get
            {
                yield return this;

                foreach (var ancestor in Ancestors)
                {
                    yield return ancestor;
                }
            }
        }

        public IEnumerable<Resource> Ancestors
        {
            get
            {
                if (Parent == null)
                    yield break;

                yield return Parent;

                foreach (var ancestor in Parent.Ancestors)
                {
                    yield return ancestor;
                }
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Name.Equals(((Resource)obj).Name);
        }

        public override string ToString()
        {
            return Name + string.Format("({{{0}}})",
                string.Join("}, {", ActionAndStrategyPairs.Select(x => x.ToString())));
        }
    }
}