namespace EdFi.Ods.Admin.UITests.Support.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Ods.Admin.UITests.Pages;
    using EdFi.Ods.Common.Utils.Extensions;

    public class PageRegistry
    {
        private static Dictionary<string, Type> _pages;
        private static object _lock= new object();

        public static PageBase GetPage(string pageName)
        {
            var searchName = pageName.Replace(" ", string.Empty);

            if (!_pages.ContainsKey(searchName))
            {
                var message = string.Format("Cannot locate page '{0}' in known pages {1}", pageName, GetKnownPages());
                throw new InvalidOperationException(message);
            }
            var pageType = _pages[searchName];
            return (PageBase) PageBase.Container.Resolve(pageType);
        }

        private static string GetKnownPages()
        {
            return string.Format("[{0}]", string.Join(", ", _pages.Keys));
        }

        public static void Initialize()
        {
            lock (_lock)
            {
                if (_pages == null)
                {
                    _pages = new Dictionary<string, Type>();
                    var types = typeof (PageRegistry).Assembly.GetTypes().Where(x => x.CanBeCastTo<PageBase>());
                    foreach (var type in types)
                    {
                        var name = type.Name.Replace("Page", string.Empty);
                        _pages[name] = type;
                    }
                }
            }
        }
    }
}