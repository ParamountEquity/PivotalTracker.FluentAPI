using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using PivotalTracker.FluentAPI.Domain;

namespace PivotalTracker.FluentAPI.Repository
{
    //TODO: Implement IterationRepo
    /// <summary>
    /// Repository that manage Iteration.
    /// </summary>
    /// <remarks>Not yet implemented</remarks>
    /// <see cref="https://www.pivotaltracker.com/help/api?version=v3#get_iterations"/>
    public class PivotalIterationRepository : PivotalTrackerRepositoryBase
    {
        #region DTOs

        [XmlRoot("iteration")]
        public class IterationXmlResponse
        {
            public int id { get; set; }
            public int number { get; set; }
            public DateTimeUTC start { get; set; }
            public DateTimeUTC finish { get; set; }
            public float team_strength { get; set; }
            public PivotalStoryRepository.StoriesXmlResponse stories { get; set; }
        }

        [XmlRoot("iterations")]
        public class IterationsXmlResponse
        {
            [XmlElement("iteration")]
            public IterationXmlResponse[] iterations;
        }

        //[XmlRoot("iteration")]
        //public class IterationsXmlRequest
        //{
        //    /* copied from project
        //    public string name { get; set; }
        //    public int iteration_length { get; set; }
        //    public DateTimeUTC first_iteration_start_time { get; set; }
        //     */
        //}


        #endregion


       
        public PivotalIterationRepository(Token token) : base(token)
        {
        }

        protected static Iteration CreateIteration(IterationXmlResponse e)
        {
            var lIteration = new Iteration
                {
                    Finish = e.finish, 
                    Id = e.id, 
                    Number = e.number, 
                    Start = e.start,
                    Team_Strength = e.team_strength
                };

            if (e.stories != null && e.stories.stories!=null)
            {
                foreach (var s in e.stories.stories)
                {
                    lIteration.Stories.Add(PivotalStoryRepository.CreateStory(s));
                }
            }


            return lIteration;
        }

       
        /// <summary>
        /// Retrieve iterations from pivotal; this will return all iterations, past present and future
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public IEnumerable<Iteration> GetIterations(int projectId)
        {
            var path = string.Format("/projects/{0}/iterations", projectId);
            var e = this.RequestPivotal<PivotalIterationRepository.IterationsXmlResponse>(path, null, "GET");
            return e.iterations.Select(PivotalIterationRepository.CreateIteration).ToList();
        }

        public IEnumerable<Iteration> GetLimitedIterations(int projectId, int offset, int limit)
        {
            throw new NotImplementedException();
        }

    }
}