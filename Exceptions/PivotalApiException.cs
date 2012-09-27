using System;

namespace PivotalTracker.FluentAPI.Exceptions
{
    public class PivotalApiException
        : Exception
    {
        public PivotalApiException(string msg) : base(msg)
        {
        }
    }
}
