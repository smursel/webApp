using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Npgsql;
using webApp.Classes;
using webApp.External;

namespace webApp.Pages.Account
{
    public class ProfileModel : PageModel
    {
        public List<VisualListing> VisualListings = new List<VisualListing>();

        private readonly DataBaseManager db = new DataBaseManager();

        [BindProperty]
        public int ListingId { get; set; }

        [BindProperty]
        public string Action { get; set; }

        public async Task OnGet()
        {
             await yap();
        }

        public async Task yap()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                Response.Redirect("/Account/Login");
            }

            string userID = HttpContext.Session.GetString("UserId");

            var user = db.FetchUserFromDatabase(int.Parse(userID));
            var myRealListings = db.FetchListings(user.Id);

            // Convert to Visual Listings
            ListingConverter converter = new(DataBaseManager._connectionString);
            var myVisualListings = new List<VisualListing>();

            foreach (var listing in myRealListings)
            {
                Console.WriteLine("123");
                myVisualListings.Add(converter.ConvertToVisualListing(listing));
            }

            VisualListings = myVisualListings;

            Console.WriteLine("nasýl yani");
        }

        public async Task<IActionResult> OnPost()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // VisualListings'i güncelle
            await yap();

            Console.WriteLine(JsonConvert.SerializeObject(VisualListings, Formatting.Indented));

            // Ýþlem yapmadan önce `ListingId` ve `Action` doðrula
            if (ListingId <= 0 || string.IsNullOrEmpty(Action))
            {
                Console.WriteLine("Invalid action or listing ID");
                return Page();
            }

            switch (Action)
            {
                case "Deactivate":
                    Console.WriteLine("deactivated");
                    DeactivateListing(ListingId);
                    break;
                case "Delete":
                    DeleteListing(ListingId);
                    break;

                    case "Activate":
                        DeactivateListing(ListingId);
                        break;
                default:
                    Console.WriteLine("Unknown action");
                    return Page();
            }

            // Ýþlemden sonra listeyi tekrar yükle
            await yap();

            return RedirectToPage("/Account/Profile");
        }

        private void DeactivateListing(int listingId)
        {
            var query = "UPDATE listings SET isactive = NOT isactive WHERE id = @ListingId";
            using var connection = new NpgsqlConnection(DataBaseManager._connectionString);
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@ListingId", listingId);

            Console.WriteLine("id :" + listingId);

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        private void DeleteListing(int listingId)
        {
            var query = "DELETE FROM listings WHERE id = @ListingId";
            using var connection = new NpgsqlConnection(DataBaseManager._connectionString);
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@ListingId", listingId);

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
