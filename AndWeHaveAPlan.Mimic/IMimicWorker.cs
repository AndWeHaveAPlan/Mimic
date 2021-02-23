using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic
{
    public interface IMimicWorker
    {
        Task<TRet> DoWork<TRet>(string mockMethodName, MockParameter[] args);
    }
}