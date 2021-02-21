using System;
using Xunit;

namespace AndWeHaveAPlan.Mimic.Tests
{
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