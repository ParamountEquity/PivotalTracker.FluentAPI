using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PivotalTracker.FluentAPI.Domain;
using PivotalTracker.FluentAPI.Service;

namespace PivotalTracker.FluentAPI.Tests
{
    [TestClass]
    public class IterationRepositoryTests
    {
        [TestMethod]
        public void GetIterationsTest()
        {
            var pivotal = new PivotalTrackerFacade(new Token(Properties.Settings.Default.ApiKey));
            var projects = pivotal.Projects().GetAll();

            foreach(var p in projects)
            {
                var i = p.Iterations();
                Assert.IsNotNull(i);

                var i2 = i.GetAll();
            }
        }
    }
}
