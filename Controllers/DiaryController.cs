using Dapper;
using DayByDay.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DayByDay.Controllers
{
    [Authorize]
    public class DiaryController : Controller
    {
        private readonly string connectionString = "";
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT * FROM diaries ORDER BY DateUpdated DESC";

            var diaries = connection.Query<Diary>(sql).ToList();

            return View(diaries);
        }

        public IActionResult DiaryDetail(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Index"); 
            }

            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT * FROM diaries WHERE Id = @id";

            var diary = connection.QuerySingleOrDefault<Diary>(sql, new { id });

            if (diary == null)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Bu id'ye sahip bir günlük bulunamadı veya erişmek için yetkiniz yok.";
                return View("Message");
            }

            return View(diary);
        }

        public IActionResult CreateDiary()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateDiary(Diary model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Günlük ekleme işlemi başarısız oldu.";
                return View("Message");
            }

            model.DateCreated = DateTime.Now;
            model.DateUpdated = DateTime.Now;

            var ImageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", ImageName);

            using var stream = new FileStream(path, FileMode.Create);
            model.Img.CopyTo(stream);

            model.ImgPath = ImageName;

            using var connection = new SqlConnection(connectionString);

            var sql = "INSERT INTO diaries (Title, Description, DateCreated, DateUpdated, ImgPath) VALUES (@Title, @Description, @DateCreated, @DateUpdated, @ImgPath)";

            var data = new
            {
                model.Title,
                model.Description,
                model.DateCreated,
                model.DateUpdated,
                model.ImgPath,
            };

            var rowsAffected = connection.Execute(sql, data);

            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Günlük ekleme işlemi başarıyla gerçekleşti.";
            return View("Message");
        }

        public IActionResult UpdateDiary(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT * FROM diaries WHERE Id = " +  id ;

            var diary = connection.QueryFirstOrDefault<Diary>(sql);
            
            return View(diary);
        }

        [HttpPost]
        public IActionResult UpdateDiary(Diary model)
        {
            using var connection = new SqlConnection(connectionString);

            var sql = "UPDATE diaries SET Title = @Title, Description = @Description, DateUpdated = @DateUpdated WHERE Id = @Id";

            var param = new
            {
                model.Title,
                model.Description,
                DateUpdated = DateTime.Now,
                model.Id,
            };

            var rowsAffected = connection.Execute(sql, param);

            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Günlük güncelleme işlemi başarıyla gerçekleşti.";
            return View("Message");
        }

        public IActionResult DeleteDiary(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var sql = "DELETE FROM diaries WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id }); 

            return RedirectToAction("Index");
        }


        //public IActionResult UpdateUserProfile(int id)
        //{
        //    using var connection = new SqlConnection(connectionString);

        //    var sql = "SELECT * FROM Users WHERE Id = @Id";

        //    var user = connection.QueryFirstOrDefault<UserProfile>(sql, new { Id = id });

        //    if (user == null)
        //    {
        //        ViewBag.MessageCssClass = "alert-danger";
        //        ViewBag.Message = "Kullanıcı bulunamadı.";
        //        return View("Message");
        //    }

        //    return View(user);
        //}

        //[HttpPost]
        //public IActionResult UpdateUserProfile(Register model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.MessageCssClass = "alert-danger";
        //        ViewBag.Message = "Lütfen tüm alanları doldurunuz.";
        //        return View(model);
        //    }

        //    using var connection = new SqlConnection(connectionString);

        //    var sql = "UPDATE Users SET Name = @Name, Surname = @Surname, Username = @Username, Email = @Email, PhoneNumber = @PhoneNumber WHERE Id = @Id";

        //    var param = new
        //    {
        //        model.Name,
        //        model.Surname,
        //        model.Username,
        //        model.Email,
        //        model.PhoneNumber,
        //        model.Id 
        //    };

        //    var rowsAffected = connection.Execute(sql, param);

        //    if (rowsAffected > 0)
        //    {
        //        ViewBag.MessageCssClass = "alert-success";
        //        ViewBag.Message = "Kullanıcı bilgileri başarıyla güncellendi.";
        //    }
        //    else
        //    {
        //        ViewBag.MessageCssClass = "alert-danger";
        //        ViewBag.Message = "Kullanıcı bilgileri güncellenirken bir hata oluştu.";
        //    }

        //    return View("Message");
        //}

        //[HttpPost]
        //public IActionResult DeleteAccount(int id)
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        var sql = "DELETE FROM Users WHERE Id = @Id";

        //        var rowsAffected = connection.Execute(sql, new { Id = userId });

        //        if (rowsAffected > 0)
        //        {
        //            HttpContext.Session.Clear();
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            ViewBag.MessageCssClass = "alert-danger";
        //            ViewBag.Message = "Hesap silme işlemi sırasında bir hata oluştu.";
        //            return View("Message");
        //        }
        //    }
        //}
    }
}