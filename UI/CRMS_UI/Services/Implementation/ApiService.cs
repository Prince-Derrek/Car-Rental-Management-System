using System.Reflection;
using System.Text;
using CRMS_UI.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CRMS_UI.Services.Implementation
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");
        }

        private void AddAuthHeader(HttpRequestMessage request, HttpContext httpContext)
        {
            var token = httpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
        }

        public async Task<T> GetAsync<T>(string endpoint, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/{endpoint}");
            AddAuthHeader(request, httpContext);
            return await SendRequestAsync<T>(request);
        }

        public async Task<T> PostAsync<T, TData>(string endpoint, TData data, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{endpoint}");
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            AddAuthHeader(request, httpContext);
            return await SendRequestAsync<T>(request);
        }

        public async Task<T> PutAsync<T, TData>(string endpoint, TData data, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/{endpoint}");
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            AddAuthHeader(request, httpContext);
            return await SendRequestAsync<T>(request);
        }

        public async Task<TResponse> PostFormAsync<TResponse, TRequest>(string endpoint, TRequest data, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{endpoint}");

            var formData = new Dictionary<string, string>();
            foreach (PropertyInfo prop in typeof(TRequest).GetProperties())
            {
                var value = prop.GetValue(data);
                if (value != null)
                {
                    formData.Add(prop.Name, value.ToString());
                }
            }

            request.Content = new FormUrlEncodedContent(formData);

            AddAuthHeader(request, httpContext);
            
            return await SendRequestAsync<TResponse>(request);
        }

        public async Task<bool> DeleteAsync(string endpoint, HttpContext httpContext)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{endpoint}");
            AddAuthHeader(request, httpContext);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
                {
                    return typeof(T) == typeof(bool) ? (T)(object)true : default(T);
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonString);
            }

            var errorJson = await response.Content.ReadAsStringAsync();
            string errorMessage = "An unknown error occurred.";

            if (!string.IsNullOrEmpty(errorJson))
            {
                try
                {
                    var errorObj = JObject.Parse(errorJson);
                    errorMessage = errorObj["message"]?.ToString() ?? errorJson;
                }
                catch
                {
                    errorMessage = "Could not parse error response.";
                }
            }

            var ex = new HttpRequestException(errorMessage, null, response.StatusCode);
            throw ex;
        }
    }
}
