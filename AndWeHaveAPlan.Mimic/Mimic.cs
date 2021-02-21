using System;

namespace AndWeHaveAPlan.Mimic
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInterface">Interface to mimic to</typeparam>
    /// <typeparam name="TMimicWorker">Real" worker class</typeparam>
    public class Mimic<TInterface, TMimicWorker> where TMimicWorker : IMimicWorker
    {
        public Type Type { get; }

        public Mimic()
        {
            Type = MimicBuilder.Create<TInterface, TMimicWorker>();
        }

        public TInterface NewInstance(TMimicWorker implementation)
        {
            var obj = (TInterface) Activator.CreateInstance(Type, implementation);
            return obj;
        }
    }
}