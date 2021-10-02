using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    //** esta clase se llama en lo primero del Startup.cs Configure method asi app.UseMiddleware<ExceptionMiddleware>();
    //** Si tratamos Exceptions con un middleware entoces es lo primero ke debe aparecer dentro de method Configure.
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //** procesamos la request a ver si se lanzan alguna exception
                await _next(context);
            }         
            catch (Exception ex)
            {
                //** log the error and prepare the response.
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                //** Si estamos en development envinronment entoces vamos a mostrar Status, Message and StackTrace
                //** Si estamos en Production envinronment entoces vamos a mostrar Status y un Message generico Internal Server Error
                var response = _env.IsDevelopment() 
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) 
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");

                //** Definimos ke vamos a mandar la response como .json y la mandamos.
                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
                var json = JsonSerializer.Serialize(response, options);
                await context.Response.WriteAsync(json);
            }   
        }
    }
}