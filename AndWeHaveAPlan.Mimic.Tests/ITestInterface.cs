using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.Tests
{
    public interface ITestInterface
    {
        int ValInt(SimpleClass input, string i2, int i3);
        Task VoidTask(SimpleClass input, string i2, int i3);
        string Str(SimpleClass input, string i2, int i3);
        Task<string> taskString(SimpleClass input, string i2, int i3);
    }
}