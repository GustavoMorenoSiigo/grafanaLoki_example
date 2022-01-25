using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace WebApp
{
    public class ExceptionMiddlewareExtensions
    {
        #region Private Fields

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        #endregion Private Fields

        #region Public Constructors

        public ExceptionMiddlewareExtensions(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task InvokeAsync(HttpContext httpContext, IDiagnosticContext diagnosticContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex, diagnosticContext);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private Task HandleExceptionAsync(HttpContext context, Exception exception, IDiagnosticContext diagnosticContext)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            diagnosticContext.Set("Details", exception.StackTrace);
            return context.Response.WriteAsync(exception.ToString());
        }

       
        private static string GetErrorsCode(string message)
        {
            StringBuilder codes = new();
            string[] errors = message.Split("\r\n -- ");
            foreach (string error in errors)
            {
                string errorCode = error.Split(":")[0];
                if (!string.IsNullOrEmpty(errorCode) && !errorCode.Equals("Validation failed"))
                    codes.Append(errorCode + ",");
            }
            return codes.ToString().Substring(0, codes.Length - 1);
        }

        #endregion Private Methods
    }
}
