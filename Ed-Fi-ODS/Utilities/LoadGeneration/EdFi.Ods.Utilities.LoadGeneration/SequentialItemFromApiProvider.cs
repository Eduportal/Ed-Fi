using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class SequentialItemFromApiProvider : IItemFromApiProvider
    {
        private ILog _logger = LogManager.GetLogger(typeof(SequentialItemFromApiProvider));

        private readonly IApiSdkFacade apiSdkFacade;

        // TODO: GKM - This needs much more granular locks for concurrency reasons, preferably on each Enumerator
        private readonly IDictionary<Type, IEnumerator> cache = new Dictionary<Type, IEnumerator>();
        private readonly object locker = new object();

        public SequentialItemFromApiProvider(IApiSdkFacade apiSdkFacade)
        {
            this.apiSdkFacade = apiSdkFacade;
        }

        public T GetNext<T>() where T : class
        {
            //This is going to be called by multi-threaded code, but the code inside here isn't thread safe.
            //Lock this whole section so we don't get any threading bugs.
            //Most of these calls are really fast, so being locked while accessing a dictionary isn't a problem,
            //But the IApiSdkFacade calls out of process, so we'll be locking during that too.  That's not good, but it won't happen often
            lock (locker)
            {
                IEnumerator enumerator = null;

                //Try fetching the enumerator from the cache
                if (cache.ContainsKey(typeof (T)))
                    enumerator = cache[typeof (T)];

                //If it's not present in the cache, go get them all, and start a new enumerator
                if (enumerator == null)
                {
                    enumerator = apiSdkFacade.GetAll(typeof(T)).GetEnumerator();
                    cache.Add(typeof (T), enumerator);
                }

                //This code uses 1 enumerator, and keeps resetting it each time it runs out.
                if (!enumerator.MoveNext())
                {
                    //If we failed to MoveNext, that means we need to reset to get more data.
                    enumerator.Reset();

                    //When we Reset(), we need to MoveNext to get to the first index
                    if (!enumerator.MoveNext())
                    {
                        //If Reset+MoveNext doesn't give us data, then it means there's no data in the enumerator.
                        _logger.WarnFormat("There is no data available for resource '{0}'.", typeof(T).Name);

                        // Nothing to return
                        return null;
                        //throw new Exception(string.Format("There is no data available for resource '{0}'.",typeof (T).Name));
                    }
                }

                return (T) enumerator.Current;
            }
        }

        public object GetNext(Type type)
        {
            var createNextMethod = this.GetType().GetMethods().Single(mi => mi.Name == "GetNext" && mi.GetGenericArguments().Any());
            var generic = createNextMethod.MakeGenericMethod(type);
            return generic.Invoke(this, null);
        }
    }
}