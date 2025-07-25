using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; 
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace X0Game.ErrorHandler
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла необработанная ошибка: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            // Создаем стандартизированный ответ
            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.",
                Details = exception.Message 
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
