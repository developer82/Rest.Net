using System.Threading.Tasks;

namespace Rest.Net.Interfaces
{
    /// <summary>
    /// Authentication interface for creating authnticators
    /// </summary>
    public interface IAuthentication
    {
        /// <summary>
        /// A method for setting up the authentication for the request
        /// </summary>
        /// <param name="request"></param>
        Task SetRequestAuthentication(IRestRequest request);
    }
}
