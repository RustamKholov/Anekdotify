using System.Net;

namespace Anekdotify.Common
{
    public class ApiResult<TSuccessData>
    {
        public bool IsSuccess { get; set; }
        public TSuccessData? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; } 

        public static ApiResult<TSuccessData> Success(TSuccessData data, HttpStatusCode statusCode)
        {
            return new ApiResult<TSuccessData> { IsSuccess = true, Data = data, StatusCode = statusCode };
        }

        public static ApiResult<TSuccessData> Failure(string errorMessage, HttpStatusCode statusCode)
        {
            return new ApiResult<TSuccessData> { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode };
        }

        public static ApiResult<TSuccessData> Failure(HttpStatusCode statusCode)
        {
            return new ApiResult<TSuccessData> { IsSuccess = false, ErrorMessage = $"HTTP Error: {statusCode}", StatusCode = statusCode };
        }
    }
}
