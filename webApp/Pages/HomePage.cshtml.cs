using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace webApp.Pages
{
    public class HomeModel : PageModel
    {
        public void OnGet()
        {
            Console.WriteLine(JsonConvert.SerializeObject(HttpContext.Session) + "perm: " + HttpContext.Session.GetString("UserRole"));

        }
    }
}
