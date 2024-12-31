namespace webApp.Classes
{
    public class Listing
    {
        public int whoCreatedid { get; set; }
        public int id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? recommendedPrice { get; set; }
        public string Description { get; set; }
        public bool? isApproved { get; set; }
        public bool isActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? PCid { get; set; }
        public string[] ImageUrls { get; set; }

        // Default constructor
        public Listing()
        {
            CreatedAt = DateTime.Now;
            isApproved = false;
            isActive = true;
        }

        // Parameterized constructor for manual assignment
        public Listing(int whoCreatedID, string name, decimal? price, decimal? recommendedPrice, string description, bool? isApproved, bool? isActive, DateTime? createdAt, DateTime? updatedAt, DateTime? approvedAt, DateTime? deletedAt, int? pcId, string[] imageUrls, int ID)
        {
            id = ID;
            this.whoCreatedid = whoCreatedID;
            this.Name = name;
            this.Price = price;
            this.recommendedPrice = recommendedPrice;
            this.Description = description;
            if (isApproved == null)
            {
                this.isApproved = false;
            }
            else
            {
                this.isApproved = isApproved;
            }

            if (isActive == null)
            {
                this.isActive = true;
            }
            else
            {
                this.isActive = isActive.Value;
            }

            this.CreatedAt = createdAt ?? DateTime.Now;
            this.UpdatedAt = updatedAt;
            this.ApprovedAt = approvedAt;
            this.DeletedAt = deletedAt;
            this.PCid = pcId;
            this.ImageUrls = imageUrls;
            isApproved = false;
            isActive = true;

        }

        public Listing CreateRandomListing(Random rand)
        {
            Listing listing = new Listing();
            listing.id = rand.Next(1, 1000);
            listing.whoCreatedid = rand.Next(1, 1000);
            listing.Name = "Random Name";
            listing.Price = rand.Next(1, 1000);
            listing.recommendedPrice = rand.Next(1, 1000);
            listing.Description = "Random Description";
            listing.isApproved = true;
            listing.isActive = true;
            listing.CreatedAt = DateTime.Now;
            listing.UpdatedAt = DateTime.Now;
            listing.ApprovedAt = DateTime.Now;
            listing.DeletedAt = DateTime.Now;

            //fetch pcId from the database

            listing.PCid = 1;
            listing.ImageUrls = new string[] { "Random URL", "Random URL" };
            return listing;

        }
    }


    public class VisualListing
    {
        public string Name { get; set; }
        public int id { get; set; }
        public bool isApproved { get; set; }
        public bool isActive { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string[] ImageUrls = new string[10];

        public string GpuName { get; set; }
        public string CpuName { get; set; }
        public string RamName { get; set; }
        public string MotherboardName { get; set; }

        public List<string> storageName { get; set; }

        public string CaseName { get; set; }

        public string PowerSupplyName { get; set; }

        public List<string> coolerNames = new List<string>();



    }

}
