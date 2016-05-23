using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    [TestFixture]
    class ResourceCountManagerTests
    {
        [Test]
        public void CanAddResources()
        {
            const string testResourceName = "TestResourceName";
            const int testResourceCount = 5;
            var resourceCountManager = new ResourceCountManager();
            resourceCountManager.InitializeResourceCount(testResourceName, testResourceCount);

            Assert.AreEqual(testResourceName, resourceCountManager.Resources.First().ResourceName);
            Assert.AreEqual(testResourceCount, resourceCountManager.Resources.First().Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CantAddResourcesWithCountOfLessThanZero()
        {
            const string testResourceName = "TestResourceName";
            const int testResourceCount = -1;
            var resourceCountManager = new ResourceCountManager();
            resourceCountManager.InitializeResourceCount(testResourceName, testResourceCount);
        }
        
        [Test]
        public void CanDecrementResourceCount()
        {
            //Setup
            const string testResourceName = "TestResourceName";
            const int testResourceCount = 5;
            var resourceCountManager = new ResourceCountManager();
            resourceCountManager.InitializeResourceCount(testResourceName, testResourceCount);

            resourceCountManager.DecrementCount("TestResourceName");
            Assert.That(resourceCountManager.Resources.First().Count == testResourceCount - 1);
        }

        [Test]
        public void CanCalculateTotalRemainingCount()
        {
            var resourceCountManager = new ResourceCountManager();
            string[] testResourceNames = {"TestResourceName", "TestResourceName2", "TestResourceName3"};
            int[] testResourceCounts = {8, 5, 3};
            for (int i = 0; i < testResourceNames.Count(); i++)
            {
                resourceCountManager.InitializeResourceCount(testResourceNames[i], testResourceCounts[i]);
            }
            Assert.AreEqual(testResourceCounts.Sum(), resourceCountManager.TotalRemainingCount);
        }

        [Test]
        public void CanGetTotalProgress()
        {
            var resourceCountManager = new ResourceCountManager();
            string[] testResourceNames = { "TestResourceName", "TestResourceName2", "TestResourceName3" };
            int[] testResourceCounts = { 1, 2, 2 };
            for (int i = 0; i < testResourceNames.Count(); i++)
            {
                resourceCountManager.InitializeResourceCount(testResourceNames[i], testResourceCounts[i]);
            }
            resourceCountManager.DecrementCount(testResourceNames.First());
            Assert.AreEqual((decimal) 0.2, resourceCountManager.TotalProgress);
        }

        [Test]
        public void Should_report_a_total_progress_of_100_percent_when_0_instances_were_originally_requested()
        {
            var resourceCountManager = new ResourceCountManager();
            resourceCountManager.InitializeResourceCount("TestResourceName", 0);
            resourceCountManager.InitializeResourceCount("TestResourceName2", 0);
            resourceCountManager.TotalProgress.ShouldEqual(1.0m);
        }

        [Test]
        public void Should_report_an_individual_resource_progress_of_100_percent_when_0_instances_of_that_resource_were_originally_requested()
        {
            var resourceCountManager = new ResourceCountManager();
            resourceCountManager.InitializeResourceCount("NoResources", 0);
            resourceCountManager.InitializeResourceCount("OneResource", 1);

            resourceCountManager.GetProgressForResource("NoResources").ShouldEqual(1.0m);
        }

        [Test]
        public void CanGetProgressForOneResource()
        {
            var resourceCountManager = new ResourceCountManager();
            string[] testResourceNames = { "TestResourceName", "TestResourceName2"};
            int[] testResourceCounts = { 43, 10};
            for (int i = 0; i < testResourceNames.Count(); i++)
            {
                resourceCountManager.InitializeResourceCount(testResourceNames[i], testResourceCounts[i]);
            }
            resourceCountManager.DecrementCount(testResourceNames.Last());
            resourceCountManager.DecrementCount(testResourceNames.Last());
            resourceCountManager.DecrementCount(testResourceNames.Last());
            Assert.AreEqual((decimal)0.3, resourceCountManager.GetProgressForResource(testResourceNames.Last()));
        }

        [Test]
        public void Cannot_decrement_resource_to_below_zero()
        {
            var resourceCountManager = new ResourceCountManager();
            
            resourceCountManager.InitializeResourceCount("TestResourceName", 1);
            resourceCountManager.DecrementCount("TestResourceName");
            Assert.AreEqual(0, resourceCountManager.Resources.Single().Count);
            
            resourceCountManager.DecrementCount("TestResourceName");
            Assert.AreEqual(0, resourceCountManager.Resources.Single().Count);
        }

        [Test]
        public void CanCalculateTotalRemainCountAfterDecrementOfResource()
        {
            var resourceCountManager = new ResourceCountManager();

            resourceCountManager.InitializeResourceCount("TestResourceName", 6);
            resourceCountManager.DecrementCount("TestResourceName");

            Assert.AreEqual(5, resourceCountManager.TotalRemainingCount);
        }

    }
}
