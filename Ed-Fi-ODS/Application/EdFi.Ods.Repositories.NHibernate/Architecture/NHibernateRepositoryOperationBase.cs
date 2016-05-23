// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common.Validation;
using NHibernate;
using NHibernate.Context;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    /// <summary>
    /// Provides session management for NHibernate-based repository implementations.
    /// </summary>
    public abstract class NHibernateRepositoryOperationBase
    {
        protected readonly ISessionFactory SessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateRepositoryOperationBase"/> class using the specified NHibernate session factory.
        /// </summary>
        /// <param name="sessionFactory"></param>
        protected NHibernateRepositoryOperationBase(ISessionFactory sessionFactory)
        {
            SessionFactory = sessionFactory;
        }

        /// <summary>
        /// Gets the current session (should call <see cref="EnsureSessionContextBinding"/> before accessing this property).
        /// </summary>
        protected ISession Session
        {
            get { return SessionFactory.GetCurrentSession(); }
        }

        /// <summary>
        /// Ensures that the current context has a session (connection) binding, opening
        /// a new session if necessary.
        /// </summary>
        /// <returns><b>true</b> if a session had to be created (and thus should be subsequently released); otherwise <b>false</b>.</returns>
        protected bool EnsureSessionContextBinding()
        {
            if (CurrentSessionContext.HasBind(SessionFactory))
                return false;

            CurrentSessionContext.Bind(SessionFactory.OpenSession());

            return true;
        }
    }

    public abstract class ValidatingNHibernateRepositoryOperationBase : NHibernateRepositoryOperationBase
    {
        private readonly IEnumerable<IObjectValidator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateRepositoryOperationBase"/> class using the specified NHibernate session factory.
        /// </summary>
        /// <param name="sessionFactory"></param>
        protected ValidatingNHibernateRepositoryOperationBase(ISessionFactory sessionFactory, IEnumerable<IObjectValidator> validators)
            : base(sessionFactory)
        {
            _validators = validators;
        }

        protected void ValidateEntity<TEntity>(TEntity entity)
        {
            var validationResults = _validators.ValidateObject(entity);

            if (!validationResults.IsValid())
            {
                throw new ValidationException(
                    string.Format("Validation of '{0}' failed.\n{1}",
                        entity.GetType().Name,
                        string.Join("\n", validationResults.GetAllMessages(indentLevel: 1))));
            }
        }
    }
}