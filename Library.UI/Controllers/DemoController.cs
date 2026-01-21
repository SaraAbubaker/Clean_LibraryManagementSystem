using Library.Common.RabbitMqMessages.UserTypeMessages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Library.UI.Controllers
{
    public class DemoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DemoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            // Get JWT from session
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // Create client and attach JWT
            var client = _httpClientFactory.CreateClient("Library.UserApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Call the UserType API
            var response = await client.GetAsync("/api/usertype/query");
            if (!response.IsSuccessStatusCode)
            {
                return Content($"❌ Failed to fetch user types: {response.StatusCode}");
            }

            var body = await response.Content.ReadAsStringAsync();
            var userTypes = JsonSerializer.Deserialize<List<UserTypeListMessage>>(body);

            // Show result
            return Content($"🎉 Authorized! Found {userTypes?.Count ?? 0} user types.");
        }

        [AllowAnonymous]
        public IActionResult Public()
        {
            return Content("Anyone can see this public demo endpoint.");
        }
    }
}
