using InternPortal.WebUI.Models; 
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace InternPortal.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
         
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7166/");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", model);

            if (response.IsSuccessStatusCode)
            {
              

                TempData["SuccessMessage"] = "Hesabınız oluşturuldu. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Kayıt sırasında bir hata oluştu.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

          
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "E-posta veya şifre hatalı.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
    public class LoginResponse { public string Token { get; set; } }
}