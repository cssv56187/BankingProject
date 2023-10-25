using Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SharedModels;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using NuGet.Common;

namespace Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7036");
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetWeatherForecast()
        {
            var client = SetBearerOnClient(_client, User); // set the jwt token on the client authorization header
            if(client != null)
            {
                var forecasts = await client.GetFromJsonAsync<IEnumerable<WeatherForecast>>("api/WeatherForecast");
                return View(forecasts);
            }
            
            return View(new List<WeatherForecast>());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private HttpClient SetBearerOnClient(HttpClient client, ClaimsPrincipal user)
        {
            if (user != null)
            {
                var identityUser = user.Identity as ClaimsIdentity;
                var jwtTokenClaim = identityUser.FindFirst("JwtToken");

                if (jwtTokenClaim != null)
                {
                    string jwtToken = jwtTokenClaim.Value;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    return client;
                }
            }
            return null;
        }
    }
}