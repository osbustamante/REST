namespace Common.Utils.RestServices
{
    using Common.Utils.Dto;
    using Common.Utils.Excepcions;
    using Common.Utils.RestServices.Interface;
    using Common.Utils.Utils.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Security;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    [ExcludeFromCodeCoverage]
    public class RestService : IRestService
    {
        #region Members

        /// <summary>
        /// The configuration
        /// </summary>
        public readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        private const string UrlControllerMethod = "{0}/{1}/{2}";
        private const string InfoException = "{0},{1},{2},{3}";
        private const string MediaTypeApplicationJson = "application/json";

        #endregion Members

        #region Builder

        public RestService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        #endregion Builder

        // Post
        public async Task<T> PostRestServiceAsync<T>(string url, string controller,
           string method, object parameters, IDictionary<string, string> headers)
        {
            try
            {
                string baseUrl;

                if (string.IsNullOrEmpty(method))
                {
                    baseUrl = string.Format("{0}/{1}", url, controller);
                }
                else
                {
                    baseUrl = string.Format(UrlControllerMethod, url, controller, method);
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeApplicationJson));

                    Microsoft.Extensions.Configuration.IConfiguration conf = _configuration.GetSection("Parameters");
                    int timeOut = Convert.ToInt32(conf.GetSection("TimeOutRest").Value);
                    client.Timeout = TimeSpan.FromMinutes(timeOut);

                    if (headers.Count > 0)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }

                    HttpContent jsonObject = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, MediaTypeApplicationJson);
                    HttpResponseMessage res = await client.PostAsync(baseUrl, jsonObject);

                    if (res.IsSuccessStatusCode)
                    {
                        var data = await res.Content.ReadAsStringAsync();
                        return JsonSerializer.Deserialize<T>(data, JsonSerializerOptionsCache.GetJsonSerializerOptions());
                    }

                    throw new BusinessException(string.Format(InfoException, baseUrl, res.StatusCode, res.Content, baseUrl));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task<T> PostRestServiceStringParametersAsync<T>(string url, string controller, string method, IDictionary<string, string> parameters, IDictionary<string, string> headers)
        {
            var urlBase = string.Format("{0}/{1}/{2}", url, controller, method);

            if (parameters.Count > 0)
                urlBase = urlBase + "?" + string.Join("&", parameters.Select(p => p.Key + "=" + p.Value).ToArray());

            using (HttpClient clientHttp = new HttpClient())
            {
                clientHttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeApplicationJson));

                if (headers.Count > 0)
                {
                    foreach (var itemHeader in headers)
                    {
                        clientHttp.DefaultRequestHeaders.Add(itemHeader.Key, itemHeader.Value);
                    }
                }

                HttpContent jsonContent = new StringContent(string.Empty);
                HttpResponseMessage resultResonse = await clientHttp.PostAsync(urlBase, jsonContent);
                if (resultResonse.IsSuccessStatusCode)
                {
                    var data = await resultResonse.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(data, JsonSerializerOptionsCache.GetJsonSerializerOptions());
                }
                else
                {
                    if (resultResonse.StatusCode == HttpStatusCode.NotFound)
                    {
                        var dataString = await resultResonse.Content.ReadAsStringAsync();
                        var jsonResponse = JsonSerializer.Deserialize<ResponseModelDto<object>>(dataString, JsonSerializerOptionsCache.GetJsonSerializerOptions());

                        throw new BusinessException(jsonResponse.Messages);
                    }
                }

                throw new ArgumentException(string.Format(InfoException, url, resultResonse.StatusCode, resultResonse.Content, urlBase));
            }
        }

        // Get
        public async Task<T> GetRestServiceAsync<T>(string url, string controller, string method,
            IDictionary<string, string> parameters, IDictionary<string, string> headers)
        {
            ValidationParameterApi(parameters);

            string baseUrlRest = string.Format(UrlControllerMethod, url, controller, method);

            if (parameters.Count > 0)
                baseUrlRest = baseUrlRest + "?" + string.Join("&", parameters.Select(p => p.Key + "=" + p.Value).ToArray());

            using (HttpClient clientResquet = new HttpClient())
            {
                clientResquet.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeApplicationJson));

                if (headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        clientResquet.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpResponseMessage httpResponse = await clientResquet.GetAsync(baseUrlRest);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var dataResult = await httpResponse.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<T>(dataResult, JsonSerializerOptionsCache.GetJsonSerializerOptions());
                }
                else
                {
                    if (httpResponse.StatusCode.Equals(HttpStatusCode.BadRequest))
                    {
                        var dataRead = await httpResponse.Content.ReadAsStringAsync();
                        var responseConvert = JsonSerializer.Deserialize<ResponseModelDto<object>>(dataRead, JsonSerializerOptionsCache.GetJsonSerializerOptions());

                        throw new BusinessException(responseConvert.Messages);
                    }
                }

                throw new ArgumentException(string.Format(InfoException, url, httpResponse.StatusCode, httpResponse.Content, baseUrlRest));
            }
        }

        private static void ValidationParameterApi(IDictionary<string, string> parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
        }

        public async Task<T> GetRestServiceAsyncList<T>(string url, string controller, string method,
          IDictionary<string, string[]> parameters, IDictionary<string, string> headers)
        {
            string endPointUrl = string.Format(UrlControllerMethod, url, controller, method);

            if (parameters.Count > 0)
                endPointUrl = endPointUrl + "?" + string.Join("&", parameters.Select(p => p.Key + "=" + p.Value).ToArray());

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeApplicationJson));

                if (headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpResponseMessage responseResultMesagge = await httpClient.GetAsync(endPointUrl);

                if (responseResultMesagge.IsSuccessStatusCode)
                {
                    var data = await responseResultMesagge.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(data, JsonSerializerOptionsCache.GetJsonSerializerOptions());
                }
                else
                {
                    if (responseResultMesagge.StatusCode.Equals(HttpStatusCode.BadRequest))
                    {
                        var stringData = await responseResultMesagge.Content.ReadAsStringAsync();
                        var resonposeJson = JsonSerializer.Deserialize<ResponseModelDto<object>>(stringData, JsonSerializerOptionsCache.GetJsonSerializerOptions());

                        throw new BusinessException(resonposeJson.Messages);
                    }
                }

                throw new ArgumentException(string.Format(InfoException, url, responseResultMesagge.StatusCode, responseResultMesagge.Content, endPointUrl));
            }
        }

    }
}