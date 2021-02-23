using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic.Tests
{
    public class MockImpl : IMimicWorker
    {
        public async Task<TRet> DoWork<TRet>(string mockMethodName, MockParameter[] args)
        {
            string input = "";
            input += args[1].Value.ToString();
            input += args[2].Value.ToString();

            var sc = ((SimpleClass) args[0].Value);

            sc.Result = input;
            sc.Input = input;
            sc.I2 = (string) args[1].Value;
            sc.I3 = (int) args[2].Value;


            if (typeof(TRet) == typeof(int))
                return (TRet) args[2].Value;

            if (typeof(TRet) == typeof(string))
                return (TRet) args[1].Value;

            return default(TRet);
        }
    }
}