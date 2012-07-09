using System;
using PivotalTracker.FluentAPI.Domain;
using PivotalTracker.FluentAPI.Repository;

namespace PivotalTracker.FluentAPI.Service
{
    public class IterationFacade : FacadeItem<IterationFacade, IterationsFacade, Iteration>
    {
        private readonly PivotalIterationRepository _repository;

        public IterationFacade(IterationsFacade parent, Iteration iteration)
            : base(parent, iteration)
        {
            _repository = new Repository.PivotalIterationRepository(this.RootFacade.Token);
        }

        //public IterationFacade Do(Action<Iteration> iteration)
        //{
        //    return this;
        //}

        //public StoriesFacade Stories()
        //{
        //    return null;

        //}
    }
}