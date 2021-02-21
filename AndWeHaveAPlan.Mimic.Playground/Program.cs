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
    public class RealWorker : IMimicWorker
    {
        public async Task<T> Mock<T>(string mockMethodName, params object[] args)
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