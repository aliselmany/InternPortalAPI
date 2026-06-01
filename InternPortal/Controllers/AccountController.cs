using InternPortal.Application.Interfaces;
using InternPortal.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace InternPortal.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMailService _mailService;

        public AccountController(IHttpClientFactory httpClientFactory, IMailService mailService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7166/");
            _mailService = mailService;
        }

        [HttpGet]
        public IActionResult Login()
        {     
            return View();
        }

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

 
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Lütfen e-posta adresinizi giriniz.";
                return View();
            }

            var response = await _httpClient.PostAsJsonAsync("api/Users/forgot-password", new { Email = email });

            if (response.IsSuccessStatusCode)
            {
                TempData["ResetEmail"] = email;
                return RedirectToAction("VerifyResetCode");
            }

            var errorResult = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            ViewBag.Error = errorResult?.Message ?? "İşlem başarısız. Lütfen e-posta adresinizi kontrol edin.";
            return View();
        }


        [HttpGet]
        public IActionResult VerifyResetCode()
        {
            if (TempData["ResetEmail"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = TempData["ResetEmail"].ToString();
            TempData.Keep("ResetEmail"); 

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyResetCode(string email, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                ViewBag.Error = "Lütfen onay kodunu giriniz.";
                ViewBag.Email = email;
                return View();
            }
            var response = await _httpClient.PostAsJsonAsync("api/Users/verify-reset-code", new { Email = email, Code = code });

            if (response.IsSuccessStatusCode)
            {
                TempData["ResetEmail"] = email;
                TempData["ResetCode"] = code;
                return RedirectToAction("ResetPassword");
            }
  
            var errorResult = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            ViewBag.Error = errorResult?.Message ?? "Girdiğiniz onay kodu hatalı veya süresi dolmuş.";
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {

            if (TempData["ResetEmail"] == null || TempData["ResetCode"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = TempData["ResetEmail"].ToString();
            ViewBag.Code = TempData["ResetCode"].ToString();

            TempData.Keep("ResetEmail");
            TempData.Keep("ResetCode");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string code, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Şifreler birbiriyle uyuşmuyor.";
                ViewBag.Email = email;
                ViewBag.Code = code;
                return View();
            }

            var model = new
            {
                Email = email,
                Code = code,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync("api/Users/reset-password", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            var errorResult = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            ViewBag.Error = errorResult?.Message ?? "İşlem başarısız. Girdiğiniz kod hatalı olabilir.";
            ViewBag.Email = email;
            ViewBag.Code = code;
            return View();
        }
    }

    public class ApiErrorResponse { public string Message { get; set; } = string.Empty; }
    public class LoginResponse { public string Token { get; set; } }
}