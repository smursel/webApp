using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using webApp.Classes;
using webApp.External;

namespace webApp.Pages
{
    public class Computerdata : PageModel
    {
        private readonly ILogger<Computerdata> _logger;
        private readonly DataBaseManager _manager = new();

        public Computerdata(ILogger<Computerdata> logger)
        {
            _logger = logger;
        }
        
        public void OnGet()
        {
              
        }

        public IActionResult OnPost()
        {
            
            return RedirectToPage("Index");
        }
    }
}
