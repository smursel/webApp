using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using System;
using System.Collections.Generic;
using webApp.Classes;
using webApp.External;

namespace webApp.Pages
{
    public class ControlListingsModel : PageModel
    {
        [BindProperty]
        public int ListingId { get; set; }

        [BindProperty]
        public string Action { get; set; }
        public List<Listing> oldListings { get; set; } = new();
        public List<VisualListing> Listings { get; set; } = new();

        public List<string> MotherboardList { get; set; } = new();
        public List<string> CpuList { get; set; } = new();
        public List<string> RamList { get; set; } = new();
        public List<string> GpuList { get; set; } = new();
        public List<string> StorageList { get; set; } = new();
        public List<string> PowerSupplyList { get; set; } = new();
        public List<string> CaseList { get; set; } = new();

        public void OnGet()
        {
            // Fetch all listings
            string query = "SELECT * FROM listings";
            using (var connection = new NpgsqlConnection(DataBaseManager._connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
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

                            var converter = new ListingConverter(DataBaseManager._connectionString);
                            Listings.Add(converter.ConvertToVisualListing(listing));

                        }
                    }
                }
            }

            // Fetch component lists
            MotherboardList = FetchComponents("motherboards");
            CpuList = FetchComponents("cpus");
            RamList = FetchComponents("rams");
            GpuList = FetchComponents("gpus");
            StorageList = FetchComponents("storages");
            PowerSupplyList = FetchComponents("powersupplies");
            CaseList = FetchComponents("cases");
        }

        public IActionResult OnPost()
        {
            switch (Action)
            {
                case "Delete":
                    RemoveListing(ListingId);
                    break;
                case "Deny":
                    DenyListing(ListingId);
                    break;
                case "Approve":
                    ApproveListing(ListingId);
                    break;

                default:
                    Console.WriteLine("Unknown action");
                    return Page();
            }


            return Page();
        }

        private void RemoveListing(int listingId)
        {
            string query = "DELETE FROM listings WHERE id = @ListingId";
            using (var connection = new NpgsqlConnection(DataBaseManager._connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListingId", listingId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void DenyListing(int listingId)
        {
            string query = "UPDATE listings SET isapproved = false WHERE id = @ListingId";
            using (var connection = new NpgsqlConnection(DataBaseManager._connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListingId", listingId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ApproveListing(int listingId)
        {
            string query = "UPDATE listings SET isapproved = true WHERE id = @ListingId";
            using (var connection = new NpgsqlConnection(DataBaseManager._connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListingId", listingId);
                    command.ExecuteNonQuery();
                }
            }
        }



        public IActionResult OnPostEditListing(int id, string name, decimal price, string description, string motherboard, string cpu, string ram, string gpu, string storage, string powerSupply, string caseItem)
        {
            try
            {
                // Fetch Component IDs
                var motherboardId = FetchComponentId("motherboards", motherboard);
                var cpuId = FetchComponentId("cpus", cpu);
                var ramId = FetchComponentId("rams", ram);
                var gpuId = FetchComponentId("gpus", gpu);
                var powerSupplyId = FetchComponentId("powersupplies", powerSupply);
                var caseId = FetchComponentId("cases", caseItem);
                var storageIds = FetchMultipleComponentIds("storages", storage);

                // Update PC details
                string pcUpdateQuery = @"
                    UPDATE pcs
                    SET motherboardid = @MotherboardId,
                        cpuid = @CpuId,
                        ramid = @RamId,
                        gpuid = @GpuId,
                        powersupplyid = @PowerSupplyId,
                        caseid = @CaseId,
                        storageids = @StorageIds
                    WHERE id = @Id";

                var pcParams = new Dictionary<string, object>
                {
                    { "@MotherboardId", motherboardId },
                    { "@CpuId", cpuId },
                    { "@RamId", ramId },
                    { "@GpuId", gpuId },
                    { "@PowerSupplyId", powerSupplyId },
                    { "@CaseId", caseId },
                    { "@StorageIds", storageIds },
                    { "@Id", id }
                };
                DataBaseManager.ExecuteNonQuery(pcUpdateQuery, pcParams);

                // Update Listing
                string listingUpdateQuery = @"
                    UPDATE listings
                    SET name = @Name,
                        price = @Price,
                        description = @Description
                    WHERE id = @Id";

                var listingParams = new Dictionary<string, object>
                {
                    { "@Name", name },
                    { "@Price", price },
                    { "@Description", description },
                    { "@Id", id }
                };
                DataBaseManager.ExecuteNonQuery(listingUpdateQuery, listingParams);

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return Page();
            }
        }

        private List<string> FetchComponents(string tableName)
        {
            var components = new List<string>();
            string query = $"SELECT name FROM {tableName}";

            using (var connection = new NpgsqlConnection(DataBaseManager._connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            components.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return components;
        }

        private int FetchComponentId(string tableName, string componentName)
        {
            string query = $"SELECT id FROM {tableName} WHERE name = @Name";

            using (var connection = new NpgsqlConnection(DataBaseManager._connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", componentName);

                    var result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : throw new Exception($"Component '{componentName}' not found in '{tableName}'");
                }
            }
        }

        private List<int> FetchMultipleComponentIds(string tableName, string componentNames)
        {
            var ids = new List<int>();
            var components = componentNames.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var component in components)
            {
                ids.Add(FetchComponentId(tableName, component.Trim()));
            }

            return ids;
        }
    }
}



