namespace EdFi.Ods.Admin.UITests.Support.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Ods.Admin.UITests.Support.Account;

    using TechTalk.SpecFlow;

    public static class FeatureContextExtensions
    {
        private const string UserAccountsKey = "UserAccounts";

        public static Dictionary<string, Account> UserAccounts(this FeatureContext context)
        {
            return (Dictionary<string, Account>)context[UserAccountsKey];
        }

        public static void ResetUserAccountCache(this FeatureContext context)
        {
            context.Set(new Dictionary<string, Account>(), UserAccountsKey);
        }

        public static Account StoreAccount(this FeatureContext context, Account account)
        {
            context.UserAccounts()[account.Email] = account;
            return account;
        }

        public static IEnumerable<Account> FindAccount(this FeatureContext context, Func<Account, bool> criteria)
        {
            return context.UserAccounts().Values.Where(criteria);
        }
    }
}