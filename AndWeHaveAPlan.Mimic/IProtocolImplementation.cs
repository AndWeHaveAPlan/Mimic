using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic
{
    public interface IProtocolImplementation
    {
        Task<TRet> MakeRequest<TRet>(string address, params object[] args);
    }
}