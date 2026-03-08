using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace WebApp.Service
{
    public abstract class HttpClientService : Controller
    {


        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        protected HttpClientService(IHttpClientFactory factory, IHttpContextAccessor ctx)
        {

            _factory = factory;
            _ctx = ctx;

        }

        protected HttpClient CreateClient()
        {
            var client = _factory.CreateClient("Gateway");
            var token = _ctx.HttpContext!.Session.GetString("accessToken");
            if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

    }
}
