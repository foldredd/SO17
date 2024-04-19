namespace ApiServiceMVC.Web.Api.Infrastructure.Loggers {
    public class LoggerMiddleware {
        private readonly RequestDelegate _next;

        public LoggerMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context) {

            Console.WriteLine("Incoming request: " + context.Request.Path);


            await _next(context);


            Console.WriteLine("Outgoing response with status code: " + context.Response.StatusCode);
        }
    }
}
