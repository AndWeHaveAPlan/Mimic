using System;
using System.Threading.Tasks;
using Xunit;

namespace AndWeHaveAPlan.Mimic.Tests
{
    public class MimicTest
    {
        [Fact]
        public void ValTypeReturn()
        {
            var tiMimic = new Mimic<ITestInterface, MockImpl>().NewInstance(new MockImpl());

            var sc = new SimpleClass();
            var inVal = tiMimic.ValInt(sc, "", 42);

            Assert.Equal(42, inVal);
        }

        [Fact]
        public async Task VoidTaskReturn()
        {
            var tiMimic = new Mimic<ITestInterface, MockImpl>().NewInstance(new MockImpl());

            var str = "val int";
            var sc = new SimpleClass();

            await tiMimic.VoidTask(sc, "10", 42);

            Assert.Equal("1042", sc.Result);
        }

        [Fact]
        public async Task StringReturn()
        {
            var tiMimic = new Mimic<ITestInterface, MockImpl>().NewInstance(new MockImpl());

            var str = "val int";
            var sc = new SimpleClass();

            var s = tiMimic.Str(sc, "10", 42);

            Assert.Equal("10", s);
        }

        [Fact]
        public async Task StringTaskReturn()
        {
            var tiMimic = new Mimic<ITestInterface, MockImpl>().NewInstance(new MockImpl());

            var str = "val int";
            var sc = new SimpleClass();

            var s = await tiMimic.taskString(sc, "10", 42);

            Assert.Equal("10", s);
        }
    }
}