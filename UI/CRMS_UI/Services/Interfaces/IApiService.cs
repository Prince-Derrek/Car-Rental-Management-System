namespace CRMS_UI.Services.Interfaces
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string endpoint, HttpContext httpContext);
        Task<T> PostAsync<T, TData>(string endpoint, TData data, HttpContext httpContext);
        Task<T> PutAsync<T, TData>(string endpoint, TData data, HttpContext httpContext);
        Task<bool> DeleteAsync(string endpoint, HttpContext httpContext);
        Task<TResponse> PostFormAsync<TResponse, TRequest>(string endpoint, TRequest data, HttpContext httpContext);
    }
}

