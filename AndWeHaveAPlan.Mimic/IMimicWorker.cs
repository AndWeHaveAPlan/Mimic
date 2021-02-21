using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic
{
    public interface IMimicWorker
    {
        Task<TRet> Mock<TRet>(string mockMethodName, MockParameter[] args);
    }
}