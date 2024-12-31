    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Npgsql;
    using System;
    using System.Globalization;
    using webApp.Classes;
    using webApp.External;

    namespace webApp.Pages.Account
    {
        public class NewListingModel : PageModel
        {
            DataBaseManager db = new DataBaseManager();
            public List<string> MotherboardList { get; set; } = new();
            public List<string> CpuList { get; set; } = new();
            public List<string> RamList { get; set; } = new();
            public List<string> GpuList { get; set; } = new();
            public List<string> StorageList { get; set; } = new();
            public List<string> PowerSupplyList { get; set; } = new();
            public List<string> CaseList { get; set; } = new();

            [BindProperty]
            public ListingModel Listing { get; set; }

            public void OnGet()
            {
                //check session
                if (HttpContext.Session.GetString("UserId") == null)
                {
                    Response.Redirect("/Account/Login");
                }



                // Veritaban�ndan bile�enleri al
                MotherboardList = FetchMotherboards();
                CpuList = FetchCPUs();
                RamList = FetchRams();
                GpuList = FetchGPUs();
                StorageList = FetchStorages();
                PowerSupplyList = FetchPowerSupplies();
                CaseList = FetchCases();
            }


            public decimal CalculateTotalComponentPrice(int? motherboardId, int? cpuId, int? ramId, int? gpuId, int? powerSupplyId, int? caseId)
            {
                decimal totalPrice = 0;

                // Helper function to fetch price by component ID
                decimal FetchPriceById(string tableName, int? componentId)
                {
                    if (!componentId.HasValue) // If component ID is null, return 0
                        return 0;

                    var query = $"SELECT COALESCE(price, 0) FROM {tableName} WHERE id = @ComponentId";
                    using (var connection = new NpgsqlConnection(DataBaseManager._connectionString))
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ComponentId", componentId.Value);
                        connection.Open();
                        var result = command.ExecuteScalar();
                        connection.Close();
                        return result != null ? Convert.ToDecimal(result) : 0;
                    }
                }

                // Fetch prices for each component by their IDs and sum them up
                totalPrice += FetchPriceById("motherboards", motherboardId);
                totalPrice += FetchPriceById("cpus", cpuId);
                totalPrice += FetchPriceById("rams", ramId);
                totalPrice += FetchPriceById("gpus", gpuId);
                totalPrice += FetchPriceById("powersupplies", powerSupplyId);
                totalPrice += FetchPriceById("cases", caseId);

                return totalPrice;
            }


            public IActionResult OnPost()
            {
                Console.WriteLine("bura");

                Console.WriteLine("bura1");

                // Kullan�c� bilgilerini oturumdan al
                var userId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                Console.WriteLine("bura2");

                // Formdan gelen veriler
                var name = Listing.Name;
                var price = Listing.Price;
                var description = Listing.Description;
                Console.WriteLine("bura3");

                // Anakart, i�lemci, RAM vb. bile�en bilgilerini al�n
                var motherboardId = FetchComponentId("motherboards", Listing.Motherboard);
                var cpuId = FetchComponentId("cpus", Listing.CPU);
                var ramId = FetchComponentId("rams", Listing.RAM);
                var gpuId = FetchComponentId("gpus", Listing.GPU);
                var powerSupplyId = FetchComponentId("powersupplies", string.Join(" ", Listing.PowerSupply.Split(" ").Skip(2)));
                var caseId = FetchComponentId("cases", Listing.Case);


                //fetch prices of all of the components from each one oftheir tables and the sum all of them then return if the price is null take it as 0

               var recommendedPrice = CalculateTotalComponentPrice(motherboardId, cpuId, ramId, gpuId, powerSupplyId, caseId);


                Console.WriteLine("bura4");
                // Depolama birimlerini i�leyin
                var storageIds = FetchMultipleComponentIds("storages", Listing.Storage);
                Console.WriteLine("bura5");

                // Resim dosyalar�n� i�le
                var uploadedImages = Listing.ImageUrls?.Select(file => SaveUploadedImage(file)).ToArray();
                Console.WriteLine("bura6");

                // Yeni bir PC kayd� olu�turun
                var pc = new PC
                {
                    motherboardId = motherboardId,
                    cpuId = cpuId,
                    ramId = ramId,
                    gpuId = gpuId,
                    powerSupplyId = powerSupplyId,
                    caseId = caseId,
                    storageIds = storageIds.ToArray()
                };
                Console.WriteLine("bura7");
                var pcId = AddPCRecord(pc);
                Console.WriteLine("bura8");

                // Yeni ilan olu�turun
                var newListing = new Listing
                {
                    whoCreatedid = userId,
                    Name = name,
                    Price = price,
                    recommendedPrice = recommendedPrice,
                    Description = description,
                    PCid = pcId,
                    ImageUrls = uploadedImages,
                    isActive = true,
                    isApproved = false,
                    CreatedAt = DateTime.Now
                };

                Console.WriteLine("bura9");

                // �lan� veritaban�na kaydedin
                SaveListingToDatabase(newListing);

                Console.WriteLine("bura10");

                return Page();
            }

            private string SaveUploadedImage(IFormFile file)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                return $"/images/{fileName}";
            }

            public static int FetchComponentId(string tableName, string componentName)
            {
                var query = "SELECT id FROM " + tableName + " WHERE name = @name";
                var parameters = new Dictionary<string, object>
                    {
                        { "@name", componentName }
                    };

                var result = DataBaseManager.ExecuteScalar(query, parameters);
                return result != null ? Convert.ToInt32(result) : throw new Exception($"Component '{componentName}' not found in table '{tableName}'");

            }

            public static List<int> FetchMultipleComponentIds(string tableName, string componentNames)
            {
                var ids = new List<int>();
                List<string> components = new();
                if (componentNames.Contains(","))
                {
                    components = componentNames.Split(',', StringSplitOptions.RemoveEmptyEntries).Take(10).ToList();
                }
                else
                {
                    components.Add(componentNames);
                }

                foreach (var component in components)
                {
                    ids.Add(FetchComponentId(tableName, String.Join(" ", component.Trim().Split(" ").Skip(1))));
                }

                return ids;
            }

            private int AddPCRecord(PC pc)
            {
                var query = @"
        INSERT INTO pcs (motherboardid, cpuid, ramid, gpuid, powersupplyid, caseid, storageids) 
        VALUES (@MotherboardId, @CpuId, @RamId, @GpuId, @PowerSupplyId, @CaseId, @StorageIds) 
        RETURNING id";

                var parameters = new Dictionary<string, object>
    {
        { "@MotherboardId", pc.motherboardId },
        { "@CpuId", pc.cpuId },
        { "@RamId", pc.ramId },
        { "@GpuId", pc.gpuId },
        { "@PowerSupplyId", pc.powerSupplyId },
        { "@CaseId", pc.caseId },
        { "@StorageIds", pc.storageIds } // `int[]` olarak g�nderilir
    };

                var pcId = DataBaseManager.ExecuteScalar(query, parameters);

                return pcId != null ? Convert.ToInt32(pcId) : throw new Exception("Failed to add PC record.");
            }

            private void SaveListingToDatabase(Listing listing)
            {
                var query = @"
        INSERT INTO listings (
            whocreatedid, name, price, recommendedprice, description, pcid, imageurls, isactive,isapproved, createdat, updatedat, approvedat, deletedat
        ) 
        VALUES (
            @WhoCreatedId, @Name, @Price, @RecommendedPrice, @Description, @PCId, @ImageUrls, @IsActive,@isApproved, @CreatedAt, '2024-12-31 14:30:00', '2024-12-31 14:30:00', '2024-12-31 14:30:00'
        )";

                var parameters = new
                {
                    WhoCreatedId = listing.whoCreatedid,           // Kim taraf�ndan olu�turuldu
                    Name = listing.Name,                           // �lan ba�l���
                    Price = listing.Price,                         // �lan fiyat�
                    RecommendedPrice = listing.recommendedPrice,   // �nerilen fiyat
                    Description = listing.Description,             // �lan a��klamas�
                    PCId = listing.PCid,                           // PC ID'si
                    ImageUrls = listing.ImageUrls,                 // G�rseller
                    IsActive = listing.isActive,                  // �lan aktif mi
                    isApproved = listing.isApproved,              // �lan onayland� m�
                    CreatedAt = listing.CreatedAt                  // Olu�turulma tarihi
                };

                // Parametrelerin loglanmas� (kontrol ama�l�)
                Console.WriteLine("Parametreler:");
                Console.WriteLine($"WhoCreatedId: {parameters.WhoCreatedId}");
                Console.WriteLine($"Name: {parameters.Name}");
                Console.WriteLine($"Price: {parameters.Price}");
                Console.WriteLine($"RecommendedPrice: {parameters.RecommendedPrice}");
                Console.WriteLine($"Description: {parameters.Description}");
                Console.WriteLine($"PCId: {parameters.PCId}");
                Console.WriteLine($"ImageUrls: {string.Join(", ", parameters.ImageUrls ?? Array.Empty<string>())}");
                Console.WriteLine($"IsActive: {parameters.IsActive}");
                Console.WriteLine($"CreatedAt: {parameters.CreatedAt}");

                // Sorguyu �al��t�r
                DataBaseManager.ExecuteNonQuery(query, parameters);

            }

            private List<string> FetchMotherboards()
            {
                var motherboards = new List<string>();

                // Veritaban�ndan anakartlar� al
                var query = "SELECT * FROM motherboards";
                var reader = db.ExecuteQueryReader(query);

                while (reader.Read())
                {
                    motherboards.Add(reader.GetString(1)); // 1: "Name" s�tunu
                }

                return motherboards;
            }

            private List<string> FetchCPUs()
            {
                var cpus = new List<string>();

                // Veritaban�ndan i�lemcileri al
                var query = "SELECT * FROM cpus";
                var reader = db.ExecuteQueryReader(query);

                while (reader.Read())
                {
                    cpus.Add(reader.GetString(1)); // 1: "Name" s�tunu
                }

                return cpus;
            }

            private List<string> FetchRams()
            {
                var rams = new List<string>();

                // Veritaban�ndan RAM'leri al
                var query = "SELECT * FROM rams";
                var reader = db.ExecuteQueryReader(query);

                while (reader.Read())
                {
                    rams.Add(reader.GetString(1)); // 1: "Name" s�tunu
                }

                return rams;
            }

            private List<string> FetchGPUs()
            {
                var gpus = new List<string>();

                // Veritaban�ndan ekran kartlar�n� al
                var query = "SELECT * FROM gpus";
                var reader = db.ExecuteQueryReader(query);

                while (reader.Read())
                {
                    gpus.Add(reader.GetString(1)); // 1: "Name" s�tunu
                }

                return gpus;
            }

            private List<string> FetchStorages()
            {
                var storages = new List<string>();

                // Veritaban�ndan depolama birimlerini al
                var query = "SELECT name,capacity FROM storages";
                var reader = db.ExecuteQueryReader(query);

                while (reader.Read())
                {
                    storages.Add(fromGbtoTB(reader.GetDouble(1)) + " " + reader.GetString(0)); // 1: "Name" s�tunu
                }

                return storages;
            }

            private string fromGbtoTB(double gb)
            {
                double tb = gb / 1024;

                // E�er 0.9'dan k���kse, GB olarak d�nd�r
                if (tb < 0.9)
                {
                    return $"{gb}GB";
                }

                // E�er 0.9'dan b�y�kse, en yak�n �eyre�e yuvarla ve TB olarak d�nd�r
                double roundedTb = Math.Round(tb * 4) / 4;

                // '.' kullanmak i�in "en-US" k�lt�r bilgisiyle formatla
                string formattedValue = roundedTb.ToString("0.##", CultureInfo.InvariantCulture);

                // Fazla s�f�rlar� kald�r
                if (formattedValue.EndsWith(".00") || formattedValue.EndsWith(".0"))
                {
                    formattedValue = formattedValue.TrimEnd('0').TrimEnd('.');
                }

                return $"{formattedValue}TB";
            }


            private List<string> FetchPowerSupplies()
            {
                var powerSupplies = new List<string>();

                // Veritaban�ndan g�� kaynaklar�n� al
                var query = "SELECT name,wattage FROM powersupplies";
                var reader = db.ExecuteQueryReader(query);

                while (reader.Read())
                {
                    powerSupplies.Add(reader.GetInt32(1) + " W " + reader.GetString(0)); // 1: "Name" s�tunu
                }

                return powerSupplies;
            }

            private List<string> FetchCases()
            {
                var cases = new List<string>();

                // Veritaban�ndan kasa bilgilerini al
                var query = "SELECT * FROM cases";
                var reader = db.ExecuteQueryReader(query);

                while (reader.Read())
                {
                    cases.Add(reader.GetString(1)); // 1: "Name" s�tunu
                }

                return cases;
            }


            private string UploadImage(IFormFile file)
            {
                if (file == null || file.Length <= 0)
                {
                    throw new ArgumentException("Ge�ersiz dosya");
                }

                // Y�kleme klas�r�n� belirle
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // Klas�r yoksa olu�tur
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Dosya ad�n� belirle
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Dosyay� sunucuya kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Resmin URL'sini d�nd�r
                return $"/uploads/{fileName}";
            }

        }

        public class ListingModel
        {
            public string Name { get; set; }
            public decimal? Price { get; set; }
            public decimal? RecommendedPrice { get; set; }
            public string Description { get; set; }
            public string Motherboard { get; set; }
            public string CPU { get; set; }
            public string RAM { get; set; }
            public string GPU { get; set; }
            public string Storage { get; set; }
            public string PowerSupply { get; set; }
            public string Case { get; set; }
            public IFormFileCollection ImageUrls { get; set; }
        }


    }
