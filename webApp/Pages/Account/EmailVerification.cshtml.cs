using Microsoft.AspNetCore.Mvc.RazorPages;
using webApp.Classes;


namespace webApp.Pages.Account
{
    public class EmailVerificationModel : PageModel
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public string ActionLink { get; set; }

        public void OnGet(string kod)
        {
            var token = TokenStore.GetToken(kod);
            var user = token == null ? null : Register.Users.Find(u => u.Email == token.Email);

            if (token == null || token.Expiration < DateTime.UtcNow || user == null)
            {
                Message = "Doğrulama kodu geçersiz veya süresi dolmuş.";
                IsSuccess = false;
                ActionLink = "<a href='/Account/Register'>Yeniden Kayıt Ol</a>";
            }
            else
            {
                user.isVerified = true;
                TokenStore.Tokens.RemoveAll(x => x.Email == user.Email);

                Message = "E-posta doğrulandı! Şimdi giriş yapabilirsiniz.";
                IsSuccess = true;
                ActionLink = "<a href='/Account/Login'>Giriş Yap</a>";
            }
        }
    }
}

