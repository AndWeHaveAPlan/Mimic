using System;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var mimic = new Mimic<IClient, RealWorker>();
            var instance = mimic.NewInstance(new RealWorker());
            instance.Foo("pickle-pee", "pump-a-rum", 42);
        }
    }

    public interface IClient
    {
        Task Foo(string s, object o, int t);
    }

    public class RealWorker : IMimicWorker
    {
        public async Task<T> Mock<T>(string mockMethodName, MockParameter[] args)
        {
            Console.WriteLine("Common worker: " + mockMethodName);

            foreach (var arg in args)
            {
                Console.WriteLine(arg.ToString());
            }

            return default;
        }
    }
}