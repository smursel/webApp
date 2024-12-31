using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using webApp.Classes;
using webApp.External;

namespace webApp.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly ILogger<DetailsModel> _logger;
        private readonly DataBaseManager _manager = new();
        public VisualListing VisualListing { get; set; }
        public List<VisualListing> RecommendedPCs = new();
        public string PhoneNumber { get; set; }

        public DetailsModel(ILogger<DetailsModel> logger)
        {
            _logger = logger;


        }

        public int? Id { get; private set; }

        public void OnGet(int? id)
        {
            Id = id;

            if (id.HasValue)
            {
                _logger.LogInformation($"ID received: {id.Value}");

                // Listing bilgilerini al
                string listingQuery = $"SELECT * FROM listings WHERE id = {id.Value}";
                using (var reader = _manager.ExecuteQueryReader(listingQuery))
                {
                    if (reader.Read())
                    {
                        // Listing nesnesini oluştur
                        Listing listing = new Listing();
                        listing.id = reader.GetInt32(0);
                        listing.Name = reader.GetString(1);
                        listing.Price = reader.GetDecimal(2);
                        listing.recommendedPrice = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3);
                        listing.Description = reader.GetString(4);
                        listing.isApproved = reader.GetBoolean(5);
                        listing.isActive = reader.GetBoolean(6);
                        listing.CreatedAt = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7);
                        listing.UpdatedAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8);
                        listing.ApprovedAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9);
                        listing.DeletedAt = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10);
                        listing.PCid = reader.GetInt32(11);
                        listing.ImageUrls = reader.IsDBNull(12) ? null : (string[])reader.GetValue(12);
                        listing.whoCreatedid = reader.GetInt32(13);

                        int userID = listing.whoCreatedid;

                        // Kullanıcı telefon numarasını al
                        var userPhoneNumberQuery = $"SELECT phonenumber FROM users WHERE id = {userID}";
                        PhoneNumber = _manager.ExecuteQuery<string>(userPhoneNumberQuery);

                        // PC bilgilerini al
                        string pcQuery = $"SELECT * FROM pcs WHERE id = {listing.PCid}";
                        using (var pcReader = _manager.ExecuteQueryReader(pcQuery))
                        {
                            if (pcReader.Read())
                            {
                                // PC nesnesini oluştur
                                PC pc = new PC
                                {
                                    id = pcReader.GetInt32(0),
                                    motherboardId = pcReader.GetInt32(1),
                                    cpuId = pcReader.GetInt32(2),
                                    ramId = pcReader.GetInt32(3),
                                    gpuId = pcReader.GetInt32(4),
                                    powerSupplyId = pcReader.GetInt32(5),
                                    caseId = pcReader.GetInt32(6),
                                    coolerIds = pcReader.GetFieldValue<int[]>(7),
                                    storageIds = pcReader.GetFieldValue<int[]>(8)
                                };

                                // Diğer bileşen bilgilerini sorgula
                                string gpuQuery = $"SELECT name FROM gpus WHERE id = {pc.gpuId}";
                                string cpuQuery = $"SELECT name FROM cpus WHERE id = {pc.cpuId}";
                                string ramQuery = $"SELECT name FROM rams WHERE id = {pc.ramId}";
                                string motherboardQuery = $"SELECT name FROM motherboards WHERE id = {pc.motherboardId}";

                                VisualListing = new VisualListing
                                {
                                    Name = listing.Name,
                                    Price = listing.Price,
                                    Description = listing.Description,
                                    ImageUrls = listing.ImageUrls,
                                    GpuName = _manager.ExecuteQuery<string>(gpuQuery),
                                    CpuName = _manager.ExecuteQuery<string>(cpuQuery),
                                    RamName = _manager.ExecuteQuery<string>(ramQuery), // RAM adını diziye ekle
                                    MotherboardName = _manager.ExecuteQuery<string>(motherboardQuery)
                                };
                            }
                            else
                            {
                                _logger.LogWarning("No PC data found for the given PC ID.");
                            }
                        }

                        // Fiyatına yakın önerilen 20 PC'yi al
                        string recommendedPCsQuery = $@"
                        SELECT id, name, price,pcId,imageUrls
                        FROM listings 
                        WHERE price IS NOT NULL 
                        ORDER BY ABS(price::numeric - {(int)listing.Price}) ASC
                        LIMIT 6";

                        var newRecommendedPCs = new List<Listing>();

                        using (var connection = new Npgsql.NpgsqlConnection(DataBaseManager._connectionString))
                        {
                            connection.Open();
                            using (var command = new Npgsql.NpgsqlCommand(recommendedPCsQuery, connection))
                            {
                                using (var xreader = command.ExecuteReader())
                                {
                                    while (xreader.Read())
                                    {
                                        newRecommendedPCs.Add(new Listing
                                        {
                                            id = xreader.GetInt32(0),
                                            Name = xreader.GetString(1),
                                            Price = xreader.GetDecimal(2),
                                            PCid = xreader.GetInt32(3),
                                            ImageUrls = xreader.IsDBNull(4) ? null : (string[])xreader.GetValue(4)
                                        });
                                    }
                                }
                            }
                        }

                        var converter = new ListingConverter(DataBaseManager._connectionString);

                        foreach (var pc in newRecommendedPCs)
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(pc, Formatting.Indented));
                            RecommendedPCs.Add(converter.ConvertToVisualListing(pc,true));
                        }

                        RecommendedPCs.ForEach(c => { 
                        
                            c.RamName = String.Join(" ", c.RamName.Split(" ").TakeLast(2));
                        
                            c.CpuName = c.CpuName.Split(" ").Last();

                            c.storageName = c.storageName.First().Split(" ").ToList();
                        });


                    }

                    else
                    {
                        _logger.LogWarning("No ID parameter received.");
                    }
                }
            }

        }
    }
}

