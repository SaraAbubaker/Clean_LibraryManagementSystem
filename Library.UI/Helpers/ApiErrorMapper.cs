using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Library.Common.DTOs.UserApiDtos.UserDtos;

namespace Library.UI.Helpers
{
    public static class ApiErrorMapper
    {
        public static void MapApiErrorToModelState(
            ModelStateDictionary modelState,
            string? message,
            string defaultError,
            Func<string, bool> fieldMapper)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                modelState.AddModelError(string.Empty, defaultError);
                return;
            }

            if (!fieldMapper(message))
            {
                modelState.AddModelError(string.Empty, message);
            }
        }

        public static void MapLoginErrorToModelState(
            ModelStateDictionary modelState,
            string? message,
            LoginUserMessage input)
        {
            MapApiErrorToModelState(
                modelState,
                message,
                "Unknown login error.",
                m =>
                {
                    if (m.Contains("password", StringComparison.OrdinalIgnoreCase))
                    {
                        modelState.AddModelError(nameof(input.Password), m);
                        return true;
                    }

                    if (m.Contains("username", StringComparison.OrdinalIgnoreCase) ||
                        m.Contains("email", StringComparison.OrdinalIgnoreCase))
                    {
                        modelState.AddModelError(nameof(input.UsernameOrEmail), m);
                        return true;
                    }

                    return false;
                });
        }

        public static void MapRegisterErrorToModelState(
            ModelStateDictionary modelState,
            string? message,
            RegisterUserMessage input)
        {
            MapApiErrorToModelState(
                modelState,
                message,
                "Unknown registration error.",
                m =>
                {
                    if (m.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                        m.Contains("confirm", StringComparison.OrdinalIgnoreCase))
                    {
                        modelState.AddModelError(nameof(input.Password), m);
                        return true;
                    }

                    if (m.Contains("username", StringComparison.OrdinalIgnoreCase))
                    {
                        modelState.AddModelError(nameof(input.Username), m);
                        return true;
                    }

                    if (m.Contains("email", StringComparison.OrdinalIgnoreCase))
                    {
                        modelState.AddModelError(nameof(input.Email), m);
                        return true;
                    }

                    return false;
                });
        }
    }
}
