using System;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            /* var something=Mimic.Create<IClient, RealWorker>();
 
             something.Foo("address11","efea fdf","324 r33343242");
             
             Console.WriteLine("Hello World!");*/
        }
    }


    public interface IClient
    {
        Task Foo(string input, string i2, string i3);
    }

    // реализация "протокола" (тут может быть всякое, например httpClient.Post(...)
    public class RealWorker : IProtocolImplementation
    {
        public async Task<T> MakeRequest<T>(string address, params object[] args)
        {
            Console.WriteLine("Common worker: " + address);

            foreach (var arg in args)
            {
                Console.WriteLine(arg.ToString());
            }

            return default;
        }
    }
}