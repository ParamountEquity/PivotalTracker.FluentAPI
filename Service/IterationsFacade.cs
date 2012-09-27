using System;
using System.Collections.Generic;
using System.Linq;
using PivotalTracker.FluentAPI.Domain;
using PivotalTracker.FluentAPI.Repository;

namespace PivotalTracker.FluentAPI.Service
{
    /// <summary>
    /// Manage a project iterations list
    /// </summary>
    /// <remarks>Not yet implemented</remarks>
    public class IterationsFacade : Facade<IterationsFacade, ProjectFacade>
    {
        private PivotalIterationRepository _iterationRepository;

        public IterationsFacade(ProjectFacade parent)
            : base(parent)
        {
        }

        
        public IEnumerable<IterationFacade> GetAll(int offset=1, int limit=10, Func<IEnumerable<Iteration>, Iteration> selector=null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IterationFacade> GetAll()
        {
            if (_iterationRepository == null)
                _iterationRepository = new PivotalIterationRepository(this.RootFacade.Token);

            var lIterations = _iterationRepository.GetIterations(ParentFacade.Item.Id);
            if (lIterations == null)
                return null;

            var lFacades = lIterations.Select(i => new IterationFacade(this, i)).ToList();
                

            return lFacades;
        }

    }
}