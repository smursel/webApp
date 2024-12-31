using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace webApp.Pages
{
    public class Logout : PageModel
    {
        private readonly ILogger<Logout> _logger;

        public Logout(ILogger<Logout> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Oturumu temizle
            HttpContext.Session.Clear();
            _logger.LogInformation("Kullan�c� oturumu kapatt�.");

            // Ana sayfaya y�nlendir
            return RedirectToPage("/HomePage");
        }
    }
}
