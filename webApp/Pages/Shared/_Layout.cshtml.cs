using Microsoft.AspNetCore.Mvc.RazorPages;
using webApp.External;

namespace webApp.Pages.Shared
{
    public class _Layout : PageModel
    {

        public bool AmILoggedIn = false;
        public string Name = "";
        DataBaseManager db = new DataBaseManager();

        public void onGet()
        {
            // Oturum bilgilerini kontrol et
            if (HttpContext.Session.GetString("UserId") != null)
            {
                // Kullanıcı giriş yapmışsa oturum bilgilerini al
                var userId = HttpContext.Session.GetString("UserId");
                var userName = HttpContext.Session.GetString("UserName");


            }
        }
        public void OnPostLogin(string username, string userId)
        {
            // Kullanıcı giriş yaparsa oturum bilgilerini ayarla
            HttpContext.Session.SetString("UserId", userId);
            HttpContext.Session.SetString("UserName", username);
        }

        public void OnPostLogout()
        {
            // Kullanıcı çıkış yaparsa oturumu temizle
            HttpContext.Session.Clear();
        }
    }
}
