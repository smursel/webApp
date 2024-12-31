using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Mail;
using webApp.Classes;

namespace webApp.Pages.Account
{
    public class Register : PageModel
    {
        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Message { get; set; }
        public bool IsSuccess { get; set; }


        private static string Domain = "https://localhost:7292";
        private static string SenderEmail = "business@lindamosep.com";
        private static string SenderPassword = "Naberbaboli*12345";

        public static List<User> Users = new List<User>();

        public void OnGet()
        {
            Message = "Lütfen bilgilerinizi doldurun.";
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Name))
            {
                Message = "Tüm alanlar doldurulmalıdır.";
                IsSuccess = false;
                return Page();
            }

            if (Users.Exists(x => x.Email == Email))
            {
                Message = "Bu e-posta zaten kayıtlı.";
                IsSuccess = false;
                return Page();
            }

            //check if password is strong enough

            if (Password.Length < 8)
            {
                Message = "Şifreniz en az 8 karakter olmalıdır.";
                IsSuccess = false;
                return Page();
            }

            if (!Password.Any(char.IsUpper) || !Password.Any(char.IsLower) || !Password.Any(char.IsDigit))
            {
                Message = "Şifreniz en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.";
                IsSuccess = false;
                return Page();
            }

            
            var result = SendVerificationEmail(Email);
            if (result)
            {
                Message = "Kayıt başarılı! Lütfen e-postanızı kontrol edin.";
                IsSuccess = true;

                Users.Add(new User
                {
                    Email = Email,
                    Password = Password,
                    Name = Name,
                    Role = "User",
                    isVerified = false
                });
            }
            else
            {
                Message = "Bir hata oluştu. Lütfen tekrar deneyin.";
                IsSuccess = false;
            }

            return Page();
        }

        private bool SendVerificationEmail(string recipientEmail)
        {
            try
            {
                string token = TokenStore.GenerateToken(recipientEmail);
                string verificationLink = $"{Domain}/Account/EmailVerification?kod={token}";

                var smtpClient = new SmtpClient("smtp.titan.email")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SenderEmail),
                    Subject = "E-posta Doğrulama",
                    Body = $"Merhaba, e-postanızı doğrulamak için aşağıdaki linke tıklayın:<br/><a href='{verificationLink}'>E-posta Doğrula</a>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipientEmail);
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public IActionResult OnGetVerifyEmail(string kod)
        {
            var token = TokenStore.GetToken(kod);
            var user = token == null ? null : Users.Find(u => u.Email == token.Email);

            if (token == null || token.Expiration < DateTime.UtcNow || user == null)
            {
                Message = "Doğrulama kodu geçersiz veya süresi dolmuş.";
                IsSuccess = false;
                return Page();
            }

            user.isVerified = true;
            TokenStore.Tokens.RemoveAll(x => x.Email == Email);

            Message = "E-posta doğrulandı! Şimdi giriş yapabilirsiniz.";
            IsSuccess = true;
            return RedirectToPage("/Account/Login");
        }
    }

}
