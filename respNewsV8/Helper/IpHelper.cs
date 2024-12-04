namespace respNewsV8.Helper
{
    public class IpHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IpHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetClientIp()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return ipAddress ?? "IP Bulunamadı";
        }
    }

}
