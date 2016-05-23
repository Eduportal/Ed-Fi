// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using NHibernate.Context;
using NHibernate.Engine;

namespace EdFi.Ods.Entities.NHibernate.Architecture
{
    [Serializable]
    public class LogicalCallSessionContext : MapBasedSessionContext
    {
        private const string SessionFactoryMapKey = "NHibernate.Context.LogicalCallSessionContext.SessionFactoryMapKey";

        public LogicalCallSessionContext(ISessionFactoryImplementor factory)
            : base(factory)
        {
        }

        /// <summary>
        /// The key is the session factory and the value is the bound session.
        /// </summary>
        protected override void SetMap(IDictionary value)
        {
            CallContext.LogicalSetData(SessionFactoryMapKey, value);
        }

        /// <summary>
        /// The key is the session factory and the value is the bound session.
        /// </summary>
        protected override IDictionary GetMap()
        {
            return CallContext.LogicalGetData(SessionFactoryMapKey) as IDictionary;
        }
    }
}