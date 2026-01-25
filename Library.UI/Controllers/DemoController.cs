using Library.Common.RabbitMqMessages.ApiResponses;
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

        [Authorize(Policy = "usertype.manage")]
        [HttpGet]
        public async Task<IActionResult> Secret()
        {
            // 🔎 Inspect all claims in the current cookie identity
            var claimsInfo = string.Join("\n", User.Claims.Select(c => $"{c.Type} = {c.Value}"));

            // Grab JWT from the authenticated user’s claims
            var token = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return Content("❌ No JWT found in claims. Did you forget to store it in SignInHelper?\n\nClaims:\n" + claimsInfo);
            }

            // Create client and attach JWT
            var client = _httpClientFactory.CreateClient("Library.UserApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Call the UserType API
            var response = await client.GetAsync("/api/usertype/query");
            if (!response.IsSuccessStatusCode)
            {
                return Content($"❌ Failed to fetch user types: {response.StatusCode}\n\nClaims:\n" + claimsInfo);
            }

            var body = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserTypeListMessage>>>(body);
            var userTypes = apiResponse?.Data;

            return Content($"🎉 Authorized! Found {userTypes?.Count ?? 0} user types.\n\nClaims:\n" + claimsInfo);
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Public()
        {
            return Content("Anyone can see this public demo endpoint.");
        }

    }
}
