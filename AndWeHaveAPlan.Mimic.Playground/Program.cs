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
        Task<int> Foo(string s, object o, int t);
    }

    public class RealWorker : IMimicWorker
    {
        public async Task<T> DoWork<T>(string mockMethodName, MockParameter[] args)
        {
            Console.WriteLine("Common worker: " + mockMethodName);

            foreach (var arg in args)
            {
                Console.WriteLine(arg.ToString());
            }

            return default;
        }
    }
    
    public class IClientMimic : IClient
    {
        private readonly RealWorker _worker;

        public IClientMimic(RealWorker worker)
        {
            _worker = worker;
        }

        public Task<int> Foo(string s, object o, int i)
        {
            return _worker.DoWork<int>(
                "Foo",
                new MockParameter[]
                {
                    new MockParameter("s", typeof(string), s),
                    new MockParameter("o", typeof(object), o),
                    new MockParameter("i", typeof(int), i)
                }
            );
        }

        public string Bar(string s)
        {
            return _worker.DoWork<string>(
                "Bar",
                new MockParameter[]
                {
                    new MockParameter("s", typeof(string), s)
                }
            ).Result;
        }
    }
}