using System;

namespace SharpMonoInjector
{
    public class InjectorException : Exception
    {
        public InjectorException(string message) : base(message)
        {
        }

        public InjectorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
