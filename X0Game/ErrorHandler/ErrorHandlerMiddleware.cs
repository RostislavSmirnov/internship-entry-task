using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace X0Game.ErrorHandler
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exceptionForHandler)
            {
                await HandleExceptionAsync(context, exceptionForHandler);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {

            ErrorResponse errorMassage = new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Timestamp = DateTime.UtcNow,
                Message = exception switch
                {
                    ArgumentNullException => "Параметр не может быть null",
                    ArgumentException => "Некорректный аргумент",
                    KeyNotFoundException => "Запись не найдена",
                    _ => "Произошла непредвиденная ошибка"
                },
                Details = exception.Message
            };

            switch (exception)
            {
                case DbUpdateException:
                    errorMassage.StatusCode = StatusCodes.Status400BadRequest;
                    errorMassage.Message = "Произошла ошибка при работе с базой данных";
                    break;
                default:
                    errorMassage.StatusCode = StatusCodes.Status500InternalServerError;
                    errorMassage.Message = "Произошла непредвиденная ошибка";
                    break;
            }

            context.Response.StatusCode = errorMassage.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorMassage));
        }
    }
}
