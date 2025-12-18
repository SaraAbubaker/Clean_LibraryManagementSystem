
namespace Library.Shared.DTOs.ApiResponses
{
    public static class ApiResponseHelper
    {
        public static ApiResponse<T> Success<T>(T data, string? message = null)
            => new ApiResponse<T> { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Failure<T>(string message)
            => new ApiResponse<T> { Success = false, Data = default, Message = message };

        public static ApiPagedResponse<T> SuccessPaged<T>(T data, string? message = null)
            => new ApiPagedResponse<T> { Success = true, Data = data, Message = message };
    }
}
