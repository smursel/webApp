using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webApp.Classes;
using webApp.External;

namespace webApp.Pages.Admin
{
    public class UploadDataModel : PageModel
    {
        [BindProperty]
        public IFormFile JsonFile { get; set; }

        private readonly DataBaseManager _manager = new();

        [BindProperty]
        public string ClassName { get; set; }

        public string ResultMessage { get; set; }
        
        public async Task<IActionResult> OnPostAsync()
        {
            var User = HttpContext.Session.GetString("User");

            if (User == null)
            {
                return RedirectToPage("/Login");
            }

            var Permission = HttpContext.Session.GetString("UserRole");

            if (Permission == null)
            {
                return RedirectToPage($"/Login");
            }

            if (Permission != "admin")
            {
                return RedirectToPage($"/Login");
            }

            if (JsonFile == null || JsonFile.Length == 0)
            {
                ResultMessage = "Lütfen bir JSON dosyası yükleyin.";
                return Page();
            }

            if (string.IsNullOrEmpty(ClassName))
            {
                ResultMessage = "Bir sınıf seçmelisiniz.";
                return Page();
            }

            string jsonData;
            using (var stream = new StreamReader(JsonFile.OpenReadStream()))
            {
                jsonData = await stream.ReadToEndAsync();
            }

            // Dinamik olarak sınıf seçimine göre deserialize işlemi
            try
            {
                switch (ClassName.ToLower())
                {
                    case "motherboard":
                        var motherboards = JsonConvert.DeserializeObject<List<Motherboard>>(jsonData);
                        _manager.InsertDataToPostgre(motherboards);
                        break;

                    case "cpu-cooler":
                        var coolers = JsonConvert.DeserializeObject<List<Cooler>>(jsonData);
                        _manager.InsertDataToPostgre(coolers);
                        break;

                    case "cpu":

                        var cpus = JsonConvert.DeserializeObject<List<CPU>>(jsonData);
                        _manager.InsertDataToPostgre(cpus);
                        break;

                    case "power-supply":
                        JArray jObject = JArray.Parse(jsonData);
                        var powerSupplies = PowerSupply.PowerSupplySerializerList(jObject);
                        _manager.InsertDataToPostgre(powerSupplies);
                        break;

                    case "video-card":
                        var gpus = JsonConvert.DeserializeObject<List<GPU>>(jsonData);
                        _manager.InsertDataToPostgre(gpus);
                        break;

                    case "case":
                        var cases = JsonConvert.DeserializeObject<List<Case>>(jsonData);
                        _manager.InsertDataToPostgre(cases);
                        break;

                    case "memory":
                        var rams = JsonConvert.DeserializeObject<List<Ram>>(jsonData);
                        _manager.InsertDataToPostgre(rams);
                        break;

                    case "internal-hard-drive":
                        var storages = JsonConvert.DeserializeObject<List<Storage>>(jsonData);
                        _manager.InsertDataToPostgre(storages);
                        break;


                    default:
                        ResultMessage = "Geçersiz sınıf seçimi.";
                        return Page();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ResultMessage = $"Veriler {ClassName} tablosuna başarıyla yüklendi.";
            return Page();
        }

    }

}

