namespace EdFi.Ods.Admin.UITests.Support.Coypu
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading;

    using global::Coypu;

    using OpenQA.Selenium;

    public static class ElementScopeExtensions
    {
        public static ElementScope WaitForAnimation(this ElementScope element, string elementName = "",
                                            bool writeDebugMessages = false)
        {
            const int movementTimeout = 100;
            string waitMessage = string.IsNullOrEmpty(elementName)
                                     ? "Waiting for element animation to complete"
                                     : string.Format("Waiting for element [{0}] animation to complete", elementName);

            var oldGeometry = Geometry(element);
            if (writeDebugMessages)
                Console.WriteLine("--- Old Geometry: {0}", oldGeometry);
            Thread.Sleep(movementTimeout);

            var newGeometry = Geometry(element);
            if (writeDebugMessages)
                Console.WriteLine("--- New Geometry: {0}", newGeometry);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var timeout = TimeSpan.FromSeconds(5);

            while (!Equals(oldGeometry, newGeometry))
            {
                if (writeDebugMessages)
                    Console.Write(waitMessage);
                Thread.Sleep(movementTimeout);
                oldGeometry = newGeometry;
                newGeometry = Geometry(element);
                if (stopwatch.Elapsed > timeout)
                    throw new Exception("Timed out waiting for animation");
            }

            return element;
        }

        public static ElementGeometry Geometry(this ElementScope element)
        {
            if (!element.Exists())
                return new ElementGeometry {Exists = false};
            var webElement = ((IWebElement) element.Native);
            Point location = webElement.Location;
            Size size = webElement.Size;
            return new ElementGeometry
                       {
                           Exists = true,
                           Height = size.Height,
                           Width = size.Width,
                           X = location.X,
                           Y = location.Y
                       };
        }
    }
}