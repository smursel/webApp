using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webApp.External;
using webApp.Classes;

namespace webApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        public DataBaseManager db = new DataBaseManager();
        public string Message { get; set; }

        public void OnGet()
        {
            Message = "Lütfen giriş yapın.";
        }

        public IActionResult OnPost(string email, string password)
        {
            // Kullanıcı doğrulama: Veritabanından kullanıcıyı kontrol et
            var user = CheckUserFromDatabase(email, password);

            if (user != null) // Kullanıcı doğruysa
            {
                // Session'a kullanıcı ID'sini ve rolünü kaydet

                Console.WriteLine("my user idddd: "+ user.Id.ToString());
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserName", user.Name);


                // Ana sayfaya yönlendir
                return RedirectToPage("/Index");
            }

            // Giriş başarısızsa hata mesajını ModelState'e ekle
            ModelState.AddModelError(string.Empty, "Geçersiz giriş bilgileri.");
            return Page();
        }

        // Kullanıcı doğrulama için örnek bir metot
        private User? CheckUserFromDatabase(string email, string password)
        {
            //check from database postgreSQL

            var user = db.CheckIfRealUser(email, password);

            return user;
        }
    }
}
