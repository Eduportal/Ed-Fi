using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EdFi.Ods.Common.Utils.Extensions;

namespace EdFi.Ods.Admin.Models.Results
{
    public abstract class AdminActionResult<TResult, TModel> : IAdminActionResult
        where TResult : AdminActionResult<TResult, TModel>
    {
        private readonly HashSet<string> _failingFields = new HashSet<string>();
        public bool Success { get; set; }
        public string RedirectRoute { get; set; }

        public string Message { get; set; }

        public TResult WithMessage(string message)
        {
            Message = message;
            return (TResult) this;
        }

        public bool HasMessage
        {
            get { return !string.IsNullOrEmpty(Message); }
        }

        public string[] FailingFields
        {
            get { return _failingFields.ToArray(); }
        }

        public TResult AddFailingField<T>(Expression<Func<TModel, T>> field)
        {
            _failingFields.Add(field.MemberName().ToLower());
            return (TResult) this;
        }
    }
}