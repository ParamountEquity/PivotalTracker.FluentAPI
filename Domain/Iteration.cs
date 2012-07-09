using System;
using System.Collections.Generic;

namespace PivotalTracker.FluentAPI.Domain
{
    /// <summary>
    /// Represent a Pivotal Iteration
    /// </summary>
    public class Iteration
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public IList<Story> Stories { get; set; }
        public float Team_Strength { get; set; }

        public Iteration()
        {
            Stories = new List<Story>();
        }
    }
}