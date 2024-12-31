using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Data;
using webApp.Classes;
using webApp.External;

namespace webApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DataBaseManager _manager = new();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        // POST ile gönderilen veriyi yakalamak için kullanılacak
        [BindProperty]
        public int ProductId { get; set; }
        public List<Listing> Listings { get; private set; }

        public List<string> CpuNames { get; private set; }
        public List<string> CoolerNames { get; private set; }
        public List<string> MotherboardNames { get; private set; }

        public List<string> CaseNames { get; private set; }
        public List<string> GpuNames = new();

        public void OnGet()
        {
            // Gelen query string'i loglama
            _logger.LogInformation(Request.QueryString.ToString());

            // CPU, GPU ve Cooler isimlerini getirme
            CpuNames = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(_manager.fetchNames("cpus")));

            string query = @"
    SELECT 
        CONCAT(SPLIT_PART(name, ' ', 1), ' ', SPLIT_PART(name, ' ', 2), ' ', SPLIT_PART(chipset, ' ', 2),SPLIT_PART(chipset, ' ', 3)) AS combined
    FROM gpus";
            GpuNames = _manager.fetchNames_ChipsetGPU(query);

            CoolerNames = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(_manager.fetchNames("coolers")));

            // Query string'den parametreleri alma
            string brand = Request.Query["Brand"];
            string cpu = Request.Query["searchQuery"];
            string cooler = Request.Query["Cooler"];
            string gpuModel = Request.Query["GpuModels"];
            string ramSize = Request.Query["ramSize"];
            string storageSize = Request.Query["storageSize"];
            string storageType = Request.Query["storageType"];
            string caseType = Request.Query["caseType"];
            decimal.TryParse(Request.Query["minPrice"], out decimal minPrice);
            decimal.TryParse(Request.Query["maxPrice"], out decimal maxPrice);

            // Filtreleri uygulayarak Listings'i atama
            Listings = FetchFilteredListings(
                brand,
                cpu,
                cooler,
                gpuModel,
                ramSize,
                storageSize,
                storageType,
                caseType,
                minPrice,
                maxPrice
            );

        }

        public List<Listing> FetchFilteredListings(
      string brand,
      string cpu,
      string cooler,
      string gpuModel,
      string ramSize,
      string storageSize,
      string storageType,
      string caseType,
      decimal minPrice,
      decimal maxPrice)
        {
            // SQL sorgusunu ve parametreleri dinamik olarak oluştur
            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(brand))
            {
                conditions.Add("EXISTS (SELECT 1 FROM cpus c WHERE c.id = p.cpuId AND c.name LIKE '%' || @brand || '%')");
                parameters.Add("brand", brand);
            }

            if (!string.IsNullOrEmpty(cpu))
            {
                conditions.Add("EXISTS (SELECT 1 FROM cpus c WHERE c.id = p.cpuId AND c.name LIKE '%' || @cpu || '%')");
                parameters.Add("cpu", cpu);
            }

            if (!string.IsNullOrEmpty(cooler))
            {
                conditions.Add(@"
        EXISTS (
            SELECT 1 
            FROM coolers cl 
            WHERE cl.id = ANY(p.coolerIds) 
            AND (
                (@cooler = 'Liquid Cooler' AND (
                    (cl.size IS NOT NULL AND cl.size ~ '^[0-9]+$' AND cl.size::integer >= 120) OR
                    cl.name ILIKE '%Liquid%' OR 
                    cl.name ILIKE '%AIO%' OR 
                    cl.name ILIKE '%Water%'
                )) OR
                (@cooler = 'Air Cooler' AND (
                    cl.size IS NULL OR 
                    NOT (cl.size ~ '^[0-9]+$' AND cl.size::integer >= 120) AND 
                    NOT cl.name ILIKE '%Liquid%' AND 
                    NOT cl.name ILIKE '%AIO%' AND 
                    NOT cl.name ILIKE '%Water%'
                ))
            )
        )");
                parameters.Add("cooler", cooler);
            }

            if (!string.IsNullOrEmpty(gpuModel))
            {
                conditions.Add("EXISTS (SELECT 1 FROM gpus g WHERE g.id = p.gpuId AND g.name LIKE '%' || @gpuModel || '%')");
                parameters.Add("gpuModel", gpuModel);
            }

            if (!string.IsNullOrEmpty(ramSize))
            {
                _logger.LogInformation("ramsize: " + ramSize);
                conditions.Add("" +
                                "EXISTS ( " +
                                    "SELECT 1 " +
                                    "FROM rams r " +
                                   "WHERE r.id = p.ramId " +
                                    $"AND r.name LIKE '% {ramSize} GB'" +
                                ")");
                parameters.Add("ramSize", int.Parse(ramSize));
            }

            if (!string.IsNullOrEmpty(storageSize))
            {
                conditions.Add("EXISTS (SELECT 1 FROM storages s WHERE s.id = ANY(p.storageIds) AND s.capacity = @storageSize::double precision)");
                parameters.Add("storageSize", double.Parse(storageSize));
            }

            if (!string.IsNullOrEmpty(storageType))
            {
                conditions.Add("EXISTS (SELECT 1 FROM storages s WHERE s.id = ANY(p.storageIds) AND s.type LIKE '%' || @storageType || '%')");
                parameters.Add("storageType", storageType);
            }

            if (!string.IsNullOrEmpty(caseType))
            {
                conditions.Add("EXISTS (SELECT 1 FROM cases cs WHERE cs.id = p.caseId AND cs.type LIKE '%' || @caseType || '%')");
                parameters.Add("caseType", caseType);
            }

            if (minPrice > 0)
            {
                conditions.Add("l.price >= @minPrice");
                parameters.Add("minPrice", minPrice);
            }

            if (maxPrice > 0)
            {
                conditions.Add("l.price <= @maxPrice");
                parameters.Add("maxPrice", maxPrice);
            }

            // Dinamik koşulları birleştir
            string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            // Nihai sorguyu oluştur
            string query = $@"
    SELECT * 
    FROM listings l
    INNER JOIN pcs p ON l.PCid = p.id
    {whereClause}";

            // Sorguyu çalıştır ve sonuçları döndür
            return FetchListingsWithReader(query, parameters).ToList();
        }


        public List<Listing> FetchListingsWithReader(string query, Dictionary<string, object> parameters)
        {
            var listings = new List<Listing>();

            using (var command = _manager.CreateCommand(query))
            {
                foreach (var param in parameters)
                {
                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = $"@{param.Key}";
                    dbParameter.Value = param.Value ?? DBNull.Value;

                    // İsterseniz parametre türlerini kontrol edip atayabilirsiniz:
                    if (param.Value is int)
                        dbParameter.DbType = DbType.Int32;
                    else if (param.Value is decimal)
                        dbParameter.DbType = DbType.Decimal;
                    else if (param.Value is string)
                        dbParameter.DbType = DbType.String;

                    command.Parameters.Add(dbParameter);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            var listing = new Listing
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                whoCreatedid = reader.GetInt32(reader.GetOrdinal("whoCreatedid")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? null : reader.GetDecimal(reader.GetOrdinal("Price")),
                                recommendedPrice = reader.IsDBNull(reader.GetOrdinal("recommendedPrice")) ? null : reader.GetDecimal(reader.GetOrdinal("recommendedPrice")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                isApproved = reader.IsDBNull(reader.GetOrdinal("isApproved")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("isApproved")),
                                isActive = reader.GetBoolean(reader.GetOrdinal("isActive")),
                                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                ApprovedAt = reader.IsDBNull(reader.GetOrdinal("ApprovedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ApprovedAt")),
                                DeletedAt = reader.IsDBNull(reader.GetOrdinal("DeletedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DeletedAt")),
                                PCid = reader.IsDBNull(reader.GetOrdinal("PCid")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PCid")),
                                ImageUrls = reader.IsDBNull(reader.GetOrdinal("ImageUrls"))
                                                                                ? null
                                                                                : reader.GetFieldValue<string[]>(reader.GetOrdinal("ImageUrls"))
                            };

                            if (listing.isApproved.HasValue)
                            {
                                if (listing.isApproved.Value)
                                {
                                    if (listing.isActive)
                                        listings.Add(listing);

                                }

                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            return listings;
        }

        public string CreateParameterizedQuery(string query, object parameters)
        {
            foreach (var prop in parameters.GetType().GetProperties())
            {
                var value = prop.GetValue(parameters) ?? "NULL";
                query = query.Replace($"@{prop.Name}", value is string ? $"'{value}'" : value.ToString());
            }
            return query;
        }


        public IActionResult OnPost()
        {
            // Butona basıldığında bu metot çalışır
            _logger.LogInformation($"Satın alma işlemi başlatıldı. Ürün ID: {ProductId}");

            // Örneğin: Başka bir sayfaya yönlendirme yapabilirsiniz
            Console.WriteLine();
            return RedirectToPage("Error");
        }
    }
}
