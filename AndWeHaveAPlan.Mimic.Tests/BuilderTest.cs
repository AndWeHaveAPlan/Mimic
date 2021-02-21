using System;
using System.Threading.Tasks;
using Xunit;

namespace AndWeHaveAPlan.Mimic.Tests
{
    public class SimpleClass
    {
        public string Input;
        public string I2;
        public int I3;

        public string Result;
    }

    public interface ITestInterface
    {
        int ValInt(SimpleClass input, string i2, int i3);
        Task VoidTask(SimpleClass input, string i2, int i3);
        string Str(SimpleClass input, string i2, int i3);
        Task<string> taskString(SimpleClass input, string i2, int i3);
    }

    public class MockImpl : IMimicWorker
    {
        public async Task<TRet> Mock<TRet>(string mockMethodName, params object[] args)
        {
            string input = "";
            input += args[1].ToString();
            input += args[2].ToString();

            var sc = ((SimpleClass) args[0]);

            sc.Result = input;
            sc.Input = input;
            sc.I2 = (string) args[1];
            sc.I3 = (int) args[2];


            if (typeof(TRet) == typeof(int))
                return (TRet) args[2];

            if (typeof(TRet) == typeof(string))
                return (TRet) args[1];

            return default(TRet);
        }
    }

    public class BuilderTest
    {
        [Fact]
        public void MimicBuild()
        {
            var type = MimicBuilder.Create<ITestInterface, MockImpl>();
            Assert.NotNull(type);
            Assert.Equal(type.Name, "ITestInterfaceMimic");
        }

        [Fact]
        public void MimicConstructed()
        {
            var type = MimicBuilder.Create<ITestInterface, MockImpl>();
            var tiMimic = (ITestInterface) Activator.CreateInstance(type, new MockImpl());

            foreach (var methodInfo in typeof(ITestInterface).GetMethods())
            {
                // TODO: check also params types
                Assert.NotNull(tiMimic.GetType().GetMethod(methodInfo.Name));
            }
        }
    }
}