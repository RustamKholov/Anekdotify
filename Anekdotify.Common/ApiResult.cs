using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.Common
{
    public class ApiResult<TSuccessData>
    {
        public bool IsSuccess { get; set; }
        public TSuccessData? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; } 

        public static ApiResult<TSuccessData> Success(TSuccessData data, System.Net.HttpStatusCode statusCode)
        {
            return new ApiResult<TSuccessData> { IsSuccess = true, Data = data, StatusCode = statusCode };
        }

        public static ApiResult<TSuccessData> Failure(string errorMessage, System.Net.HttpStatusCode statusCode)
        {
            return new ApiResult<TSuccessData> { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode };
        }

        public static ApiResult<TSuccessData> Failure(System.Net.HttpStatusCode statusCode)
        {
            return new ApiResult<TSuccessData> { IsSuccess = false, ErrorMessage = $"HTTP Error: {statusCode}", StatusCode = statusCode };
        }
    }
}
