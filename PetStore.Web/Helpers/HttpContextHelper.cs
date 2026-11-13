using Microsoft.AspNetCore.Http;

namespace PetStore.Helpers
{
    // gives us back HttpContext.Current like the good old days
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor _accessor;

        public static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public static HttpContext Current
        {
            get { return _accessor == null ? null : _accessor.HttpContext; }
        }
    }
}
