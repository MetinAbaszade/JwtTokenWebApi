namespace JwtTokenWebApi.MiddleWare
{

    public class AccessTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Read the access token from the cookie
            var accessToken = context.Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                // Add the access token to the Authorization header
                context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            }

            await _next(context);
        }
    }
}
