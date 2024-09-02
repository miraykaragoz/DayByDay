using Dapper;
using DayByDay.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;

namespace DayByDay.Controllers
{
    public class AccountController : Controller
    {
        private readonly string connectionString = "";

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Diary");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Diary");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Kullanıcı adı veya şifre boş bırakılamaz.";
                return View("Message");
            }

            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT Id, Username, Password FROM Users WHERE Username = @Username";

            var user = connection.QueryFirstOrDefault<Login>(sql, new { Username = model.Username });

            if (user != null && user.Password == model.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);

                return RedirectToAction("Index", "Diary");
            }
            else
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Kullanıcı adı veya şifre hatalı.";
                return View("Message");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Diary");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Register(Register model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Kullanıcı kaydı başarısız oldu.";
                return View("Message");
            }

            using var connection = new SqlConnection(connectionString);

            var sql = "INSERT INTO Users (Name, Surname, Username, Email, PhoneNumber, Password) VALUES (@Name, @Surname, @Username, @Email, @PhoneNumber, @Password)";

            var data = new
            {
                model.Name,
                model.Surname,
                model.Username,
                model.Email,
                model.PhoneNumber,
                model.Password
            };

            var rowsAffected = connection.Execute(sql, data);

            using var reader = new StreamReader("wwwroot/mailTemp/mail.html");
            var template = reader.ReadToEnd();
            var mailBody = template;

            var client = new SmtpClient("smtp.eu.mailgun.org", 587)
            {
                Credentials = new NetworkCredential("postmaster@bilgi.miraykaragoz.com.tr", ""),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("postmaster@bilgi.miraykaragoz.com.tr", "DayByDay"),
                Subject = "Hoşgeldiniz!",
                Body = mailBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(model.Email, "Değerli kullancı,"));

            client.Send(mailMessage);

            if (rowsAffected > 0)
            {
                ViewBag.MessageCssClass = "alert-success";
                ViewBag.Message = "Kullanıcı kaydı başarıyla gerçekleşti.";
            }
            else
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Kullanıcı kaydı sırasında bir hata oluştu.";
            }

            return View("Message");
        }

        public IActionResult UserProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT Name, Surname, Username, Email, PhoneNumber FROM Users WHERE Id = @Id";

            var user = connection.QueryFirstOrDefault<UserProfile>(sql, new { Id = userId });

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }
    }
}