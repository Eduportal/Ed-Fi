using System.Collections;
using System.Collections.Generic;

namespace EdFi.Ods.BulkLoad.Common
{
    public static class OrderedCollectionOfSetsExtensions
    {
        public static IEnumerable<T> GetAllContainedMembers<T>(this OrderedCollectionOfSets<T> collection)
        {
            var listofT = new List<T>();
            foreach (var set in collection)
            {
                foreach (var t in set)
                {
                    listofT.Add(t);
                }
            }
            return listofT;
        }
    }

    public class OrderedCollectionOfSets<T> : IEnumerable<ISet<T>>
    {
        private readonly IList<ISet<T>> _internalCollection = new List<ISet<T>>();

        public IEnumerator<ISet<T>> GetEnumerator()
        {
            return _internalCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public SetBuilder AddSet()
        {
            var builder = new SetBuilder(this);
            _internalCollection.Add((HashSet<T>) builder);
            return builder;
        }

        public ISet<T> RemoveSet(int index)
        {
            if (index > (_internalCollection.Count - 1)) return null;
            var requestedList = _internalCollection[index];
            _internalCollection.RemoveAt(index);
            return requestedList;
        }

        public class SetBuilder
        {
            private readonly OrderedCollectionOfSets<T> _parent;
            private readonly HashSet<T> _internalSet = new HashSet<T>();

            public SetBuilder(OrderedCollectionOfSets<T> parent)
            {
                _parent = parent;
            }

            public static implicit operator HashSet<T>(SetBuilder builder)
            {
                return builder._internalSet;
            }

            public static implicit operator OrderedCollectionOfSets<T>(SetBuilder builder)
            {
                return builder._parent;
            }

            public SetBuilder AddMember(T member)
            {
                _internalSet.Add(member);
                return this;
            }

            public SetBuilder AddMemberRange(IEnumerable<T> members)
            {
                foreach (var member in members)
                {
                    _internalSet.Add(member);
                }
                return this;
            }

            public SetBuilder AddSet()
            {
                return _parent.AddSet();
            }
        }
    }
}