using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webApp.Classes;
using webApp.External;
using Microsoft.AspNetCore.Session;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi (örneðin 30 dakika)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor kaydý

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

    DataBaseManager db = new DataBaseManager();
try
{
    // Veritabaný iþlemleri

    // SQL sorgusu ile "cases" tablosundaki tüm verilerden farklý olan type'larý getir
    var query = "SELECT type, COUNT(*) AS count FROM cases GROUP BY type;";
    var caseTypes = db.ExecuteQueryReader(query);

    while (caseTypes.Read())
    {
        Console.WriteLine(caseTypes.GetString(0) + " - " + caseTypes.GetInt32(1));
    }

}
catch (Exception e)
{
    Console.WriteLine(e);
}

try
{
    // JSON dosyalarýndan veri okuma (örnek kullaným)
    var AllExistingFiles = Directory.GetFiles(Environment.CurrentDirectory + "\\jsons");

    foreach (var c in AllExistingFiles)
    {
        var jsonData = System.IO.File.ReadAllText(c);
        string FileName = c.Split("\\").Last().Split(".").First();

        switch (FileName)
        {
            // Yorum satýrýna alýnmýþ örnekler:
          //  case "motherboard":
          //      var motherboards = JsonConvert.DeserializeObject<List<Motherboard>>(jsonData);
          //      db.InsertDataToPostgre(motherboards);
          //      break;
          //
          //  case "cpu-cooler":
          //      var jArray = JArray.Parse(jsonData);
          //      var coolers = Cooler.CoolerSerializerList(jArray);
          //      db.InsertDataToPostgre(coolers);
          //      break;
          //
          //  case "cpu":
          //      var cpus = JsonConvert.DeserializeObject<List<CPU>>(jsonData);
          //      Console.WriteLine(JsonConvert.SerializeObject(cpus, Formatting.Indented));
          //      db.InsertDataToPostgre(cpus);
          //      break;
          //
          //  case "power-supply":
          //      var powerSupplies = PowerSupply.PowerSupplySerializerList(JArray.Parse(jsonData));
          //      db.InsertDataToPostgre(powerSupplies);
          //      break;
          //
          //  case "video-card":
          //      db.ExecuteQuery<object>($"DELETE FROM gpus");
          //      var gpus = JsonConvert.DeserializeObject<List<GPU>>(jsonData);
          //      db.InsertDataToPostgre(gpus);
          //      break;
          //
          //  case "case":
          //      var cases = JsonConvert.DeserializeObject<List<Case>>(jsonData);
          //      db.InsertDataToPostgre(cases);
          //      break;
          //
          //  case "memory":
          //      var rams = Ram.RamSerializerList(JArray.Parse(jsonData));
          //      db.InsertDataToPostgre(rams);
          //      break;
          //
          //  case "internal-hard-drive":
          //      var storages = JsonConvert.DeserializeObject<List<Storage>>(jsonData);
          //      db.InsertDataToPostgre(storages);
          //      break;
          //
          //  default:
          //      Console.WriteLine($"Unsupported file: {FileName}");
          //      break;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session kullanýmý

app.UseAuthorization();

app.MapRazorPages();

app.Run();
