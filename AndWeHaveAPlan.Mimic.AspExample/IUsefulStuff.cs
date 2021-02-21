using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.AspExample
{
    public interface IUsefulStuff
    {
        Task<string> GetSomeValue(string key);
        Task SetSomeValue(string key, string value);
    }
}