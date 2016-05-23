namespace EdFi.Ods.Admin.UITests.Pages
{
    using System;
    using System.Text.RegularExpressions;

    using BoDi;

    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;
    using EdFi.Ods.Admin.UITests.Support;
    using EdFi.Ods.Admin.UITests.Support.Coypu;
    using EdFi.Ods.Common.Utils.Extensions;

    using OpenQA.Selenium;

    using TechTalk.SpecFlow;

    /// <summary>
    ///     Provides base level functionality for implementing the Page object pattern against the Ed-Fi Dashboards.
    /// </summary>
    public abstract class PageBase
    {
        protected PageBase(BrowserSession browser)
        {
            this.Browser = browser;
        }

        /// <summary>
        ///     Gets the native WebDriver implementation for the browser session based on the user profile of the current scenario.
        /// </summary>
        protected IWebDriver Driver
        {
            get { return this.Browser.Driver.Native as IWebDriver; }
        }

        /// <summary>
        ///     Gets the browser session based on the user profile of the current scenario.
        /// </summary>
        public BrowserSession Browser { get; private set; }

        ///// <summary>
        ///// Gets the REST client based on the user profile of the current scenario.
        ///// </summary>
        //public RestClient RestClient
        //{
        //    get
        //    {
        //        // Get REST client for current user
        //        return ScenarioContext.Current.GetRestClient();
        //    }
        //}

        /// <summary>
        ///     Gets the IoC container for the current scenario.
        /// </summary>
        public static IObjectContainer Container
        {
            get
            {
                // Get the current scenario's IoC container
                return ScenarioContext.Current.GetBindingInstance(typeof (IObjectContainer)) as IObjectContainer;
            }
        }

        public static TestUrlBuilder UrlBuilder
        {
            get { return Container.Resolve<TestUrlBuilder>(); }
        }

        /// <summary>
        ///     Identifies whether the webpage represented by the Page object is the current webpage in the browser.
        /// </summary>
        /// <returns>
        ///     <b>true</b> if the Page object represents the current browser webpage; otherwise <b>false</b>.
        /// </returns>
        public virtual bool IsCurrent(Options options = null, bool showDiagnostics = false)
        {
            var attributes = this.GetType().GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                // TODO: GKM - Determine whether to add support for ASP.NET MVC routing system to support this attribute
                //var associatedControllerAttribute = attribute as AssociatedControllerAttribute;

                //if (associatedControllerAttribute != null)
                //{
                //    Type[] associatedControllerTypes = associatedControllerAttribute.ControllerTypes;

                //    return GetIsCurrentRobustly(associatedControllerTypes, options, showDiagnostics);
                //}

                var associatedUrlAttribute = attribute as AssociatedUrlAttribute;

                if (associatedUrlAttribute != null)
                {
                    string pattern = associatedUrlAttribute.RegexPattern;

                    if (Regex.IsMatch(this.Browser.Location.AbsolutePath, pattern))
                        return true;

                    return false;
                }
            }

            string message = string.Format("Page class '{0}' does not have a supported 'AssociatedXxxx' attribute for identifying the associated web page.", this.GetType());
            throw new NotImplementedException(message);
        }

        protected virtual void Visit(string virtualPath)
        {
            this.Browser.Visit(UrlBuilder.BuildUrl(virtualPath));
        }

        private bool GetIsCurrentRobustly(Type[] associatedControllerTypes, Options options, bool showDiagnostics)
        {
            throw new NotImplementedException();

            // TODO: Make this real for this context
            //DateTime startTime = DateTime.Now;

            //bool isCurrent = false;
            //Type mappedControllerType;

            //var actualOptions = options ?? Make_It.Do_It_Now;

            //do
            //{
            //    string virtualPath = Browser.Location.AbsoluteUri.ToVirtual();

            //    // Make sure path got virtualized.  If not, it's not on the website, and we're done
            //    if (!virtualPath.StartsWith("~"))
            //        return false;

            //    mappedControllerType = virtualPath.Route().GetMappedControllerType();

            //    foreach (var associatedControllerType in associatedControllerTypes)
            //    {
            //        isCurrent |= mappedControllerType == associatedControllerType;
            //    }

            //    // If we're not current, delay momentarily before retrying
            //    if (!isCurrent)
            //        Thread.Sleep(actualOptions.RetryInterval);
            //} while ((DateTime.Now - startTime) <= actualOptions.Timeout);

            //// For debugging:
            //if (!isCurrent && showDiagnostics)
            //{
            //    Debug.WriteLine("{0} is not the current page.\n\tExpected: {1}\n\tActual: {2}",
            //        this.GetType().Name, 
            //        string.Join(", ", associatedControllerTypes.Select(t => t.FullName)), 
            //        mappedControllerType.FullName);
            //}

            //return isCurrent;
        }

        /// <summary>
        ///     Navigates to the webpage represented by the Page object.
        /// </summary>
        public abstract void Visit();


        /// <summary>
        ///     Attempts to click a link with the specified text.
        /// </summary>
        /// <param name="linkText">The text of the link to find and click.</param>
        /// <returns>
        ///     <b>true</b> if the link was found and clicked; otherwise <b>false</b>.
        /// </returns>
        private static bool TryClickLink(BrowserSession browser, string linkText, Options options = null)
        {
            if (options == null)
                options = Make.Options.Wait(1.Seconds());

            try
            {
                var link = browser.FindLink(linkText, options);
                link.Click();

                return true;
            }
            catch (MissingHtmlException)
            {
                return false;
            }
        }

        public bool HasContent(string text)
        {
            return this.Browser.HasContent(text);
        }

        protected virtual void TrimStrings(ref string string1)
        {
            string1 = string1.Trim();
        }

        protected virtual void TrimStrings(ref string string1, ref string string2)
        {
            string1 = string1.Trim();
            string2 = string2.Trim();
        }

        protected virtual void TrimStrings(ref string string1, ref string string2, ref string string3)
        {
            string1 = string1.Trim();
            string2 = string2.Trim();
            string3 = string3.Trim();
        }

        protected virtual void TrimStrings(ref string string1, ref string string2, ref string string3, ref string string4)
        {
            string1 = string1.Trim();
            string2 = string2.Trim();
            string3 = string3.Trim();
            string4 = string4.Trim();
        }

        // This method can be used on any page, and therefore should be anonymous. Im not sure it belongs here. -BKM
    }
}