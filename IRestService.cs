using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.RestServices.Interface
{
    public interface IRestService
    {
        Task<T> GetRestServiceAsync<T>(string url, string controller, string method,
            IDictionary<string, string> parameters, IDictionary<string, string> headers);

        Task<T> PostRestServiceAsync<T>(string url, string controller, string method
            , object parameters, IDictionary<string, string> headers);

        Task<T> GetRestServiceAsyncList<T>(string url, string controller, string method,
          IDictionary<string, string[]> parameters, IDictionary<string, string> headers);

        Task<T> PostRestServiceStringParametersAsync<T>(string url, string controller, string method
            , IDictionary<string, string> parameters, IDictionary<string, string> headers);

    }
}