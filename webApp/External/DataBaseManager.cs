using Newtonsoft.Json;
using Npgsql;
using System.Data.Common;
using webApp.Classes;

namespace webApp.External
{
    public class DataBaseManager
    {
        public static readonly string _connectionString = "Host=localhost;Port=5432;Username=mursel;Password=naberabla121224;Database=YazilimMuhProject;Include Error Detail=true";
        private static NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

        public void getAllComputers()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var listings = new List<Listing>();
                using (var command = new NpgsqlCommand("SELECT * FROM Computers", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var listing = new Listing(
                                whoCreatedID: reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                                name: reader.GetString(reader.GetOrdinal("Name")),
                                price: reader.IsDBNull(reader.GetOrdinal("Price"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("Price")),
                                recommendedPrice: reader.IsDBNull(reader.GetOrdinal("RecommendedPrice"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("RecommendedPrice")),
                                description: reader.GetString(reader.GetOrdinal("Description")),
                                isApproved: reader.IsDBNull(reader.GetOrdinal("IsApproved"))
                                    ? null
                                    : reader.GetBoolean(reader.GetOrdinal("IsApproved")),
                                isActive: reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                createdAt: reader.IsDBNull(reader.GetOrdinal("CreatedAt"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                updatedAt: reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                approvedAt: reader.IsDBNull(reader.GetOrdinal("ApprovedAt"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("ApprovedAt")),
                                deletedAt: reader.IsDBNull(reader.GetOrdinal("DeletedAt"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("DeletedAt")),
                                pcId: reader.IsDBNull(reader.GetOrdinal("PCId"))
                                    ? null
                                    : reader.GetInt32(reader.GetOrdinal("PCId")),
                                imageUrls: reader.IsDBNull(reader.GetOrdinal("ImageUrls"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("ImageUrls")).Split(','),
                                ID: reader.GetInt32(reader.GetOrdinal("Id"))
                            );

                            listings.Add(listing);
                        }
                    }
                }
            }
        }

        public List<string> fetchNames_ChipsetGPU(string query)
        {
            var combinedList = new List<string>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            combinedList.Add(reader["combined"].ToString());
                        }
                    }
                }
            }

            return combinedList;

        }


        public List<Listing> FetchListings(int id)
        {

            //fetch listings from database from npsql and return them as a list

            List<Listing> listings = new List<Listing>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM listings WHERE whoCreatedID = @id", connection))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Listing listing = new
                                (
                                                            reader.GetInt32(13),
                            reader.GetString(1),
                            reader.GetDecimal(2),
                            reader.GetDecimal(3),
                            reader.GetString(4),
                            reader.GetBoolean(5),
                            reader.GetBoolean(6),
                            reader.GetDateTime(7),
                            reader.GetDateTime(8),
                            reader.GetDateTime(9),
                            reader.GetDateTime(10),
                            reader.GetInt32(11),
                            reader.GetValue(12) as string[],
                                                        reader.GetInt32(0)

                                );
                            listings.Add(listing);
                        }

                    }

                    return listings;

                }
            }


        }

        public object fetchNames(string tableName)
        {
            //fetch names from the database and return them as a list
            List<string> names = new List<string>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                Console.WriteLine(tableName);
                connection.Open();

                string name = "name";

                switch (tableName)
                {
                    case "cpus":
                        name = "Name";
                        break;
                    case "gpus":
                        name = "chipset";
                        break;
                    case "rams":
                        name = "Name";
                        break;
                    case "coolers":
                        name = "Name";
                        break;
                    case "motherboards":
                        name = "Name";
                        break;
                    case "powersupplies":
                        name = "Name";
                        break;
                    case "storages":
                        name = "Name";
                        break;
                    case "videocards":
                        name = "Name";
                        break;
                    case "cases":
                        name = "Name";
                        break;
                }

                using (var cmd = new NpgsqlCommand($"SELECT {name} FROM {tableName}", connection))
                {
                    cmd.Parameters.AddWithValue("tableName", tableName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            names.Add(reader.GetString(0));
                        }
                    }
                }
                connection.Close();
            }

            return names;
        }

        public object fetchNames<T>()
        {
            //fetch names from the database and return them as a list
            string tableName = typeof(T).Name + "s";
            List<string> names = new List<string>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand($"SELECT Name FROM {tableName}", connection))
                {
                    cmd.Parameters.AddWithValue("tableName", tableName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            names.Add(reader.GetString(1));
                        }
                    }
                }

                connection.Close();
            }



            return names;
        }

        public void CheckAndCreatePCsTable()
        {
            if (!TableExists("pcs"))
            {
                string createPCsTableQuery = @"
                CREATE TABLE pcs (
                    Id SERIAL PRIMARY KEY,
                    motherboardId INT NOT NULL,
                    cpuId INT NOT NULL,
                    ramId INT NOT NULL,
                    gpuId INT NOT NULL,
                    powerSupplyId INT NOT NULL,
                    caseId INT NOT NULL,
                    coolerIds INT[],
                    storageIds INT[],
                    imageUrls TEXT[],
                    CONSTRAINT fk_motherboard FOREIGN KEY (motherboardId) REFERENCES motherboards(Id) ON DELETE CASCADE,
                    CONSTRAINT fk_cpu FOREIGN KEY (cpuId) REFERENCES cpus(Id) ON DELETE CASCADE,
                    CONSTRAINT fk_ram FOREIGN KEY (ramId) REFERENCES rams(Id) ON DELETE CASCADE,
                    CONSTRAINT fk_gpu FOREIGN KEY (gpuId) REFERENCES gpus(Id) ON DELETE CASCADE,
                    CONSTRAINT fk_powerSupply FOREIGN KEY (powerSupplyId) REFERENCES powersupplies(Id) ON DELETE CASCADE,
                    CONSTRAINT fk_case FOREIGN KEY (caseId) REFERENCES cases(Id) ON DELETE CASCADE
                );
            ";

                ExecuteNonQuery(createPCsTableQuery);
                Console.WriteLine("Table 'pcs' created with all necessary foreign keys.");
            }
        }

        public void CheckAndCreateRelatedTables()
        {
            CreateRelatedTableIfNotExists("motherboards");
            CreateRelatedTableIfNotExists("cpus");
            CreateRelatedTableIfNotExists("rams");
            CreateRelatedTableIfNotExists("gpus");
            CreateRelatedTableIfNotExists("powersupplies");
            CreateRelatedTableIfNotExists("cases");
        }

        private static void CreateRelatedTableIfNotExists(string tableName)
        {
            if (!TableExists(tableName))
            {
                string createTableQuery = $@"
                CREATE TABLE {tableName} (
                    Id SERIAL PRIMARY KEY,
                    Name TEXT NOT NULL
                );
            ";

                ExecuteNonQuery(createTableQuery);
                Console.WriteLine($"Table '{tableName}' created.");
            }
        }

        private static bool TableExists(string tableName)
        {
            string checkTableQuery = $@"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = '{tableName}'
            );
        ";


            using (var checkCommand = new NpgsqlCommand(checkTableQuery, connection))
            {
                connection.Open();
                var returner = (bool)checkCommand.ExecuteScalar();
                connection.Close();
                return returner;
            }
        }

        public DbCommand CreateCommand(string query)
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand(); // _connection, DbConnection türündedir
            command.CommandText = query;
            return command;
        }


        public NpgsqlDataReader ExecuteQueryReader(string query)
        {
            var newConnection = new NpgsqlConnection(_connectionString); // Yeni bağlantı
            newConnection.Open();

            try
            {
                var command = new NpgsqlCommand(query, newConnection);
                return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }



        public T ExecuteQuery<T>(string query)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            return default;
                        }

                        if (result is T castResult)
                        {
                            return castResult;
                        }

                        throw new InvalidCastException($"Cannot cast result of type {result.GetType()} to {typeof(T)}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw;
                }
            }
        }
        public static void ExecuteNonQuery(string query, object parameters = null)
        {
            using (var command = new NpgsqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    foreach (var property in parameters.GetType().GetProperties())
                    {
                        command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(parameters));
                    }
                }

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static object ExecuteScalar(string query, Dictionary<string, object> parameters)
        {
            using (var command = new NpgsqlCommand(query, connection))
            {
                // Parametreleri ekleyin
                foreach (var param in parameters)
                {
                    if (param.Value is int[] array)
                    {
                        command.Parameters.Add(param.Key, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer)
                              .Value = array;
                    }
                    else
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                connection.Open();
                var result = command.ExecuteScalar();
                connection.Close();

                return result;
            }
        }


        public static void ExecuteNonQuery(string query)
        {
            using (var command = new NpgsqlCommand(query, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void CreateListing(Listing listing)
        {


            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string checkTableQuery = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = 'listings'
                );
            ";

                bool tableExists;
                using (var checkCommand = new NpgsqlCommand(checkTableQuery, connection))
                {
                    tableExists = (bool)checkCommand.ExecuteScalar();
                }

                // If the table does not exist, create it
                if (!tableExists)
                {
                    string createTableQuery = @"
        CREATE TABLE listings (
            Id SERIAL PRIMARY KEY,
            whoCreatedID INT NOT NULL,
            Name TEXT NOT NULL,
            Price DECIMAL,
            recommendedPrice DECIMAL,
            Description TEXT,
            isApproved BOOLEAN,
            isActive BOOLEAN NOT NULL,
            CreatedAt TIMESTAMP,
            UpdatedAt TIMESTAMP,
            ApprovedAt TIMESTAMP,
            DeletedAt TIMESTAMP,
            PCId INT,
            ImageUrls TEXT[],
            CONSTRAINT fk_pc FOREIGN KEY (PCId) REFERENCES PCs(Id) ON DELETE CASCADE
        );
    ";

                    using (var createCommand = new NpgsqlCommand(createTableQuery, connection))
                    {
                        createCommand.ExecuteNonQuery();
                        Console.WriteLine("Table 'listings' created.");
                    }
                }

                // INSERT INTO query corrected to match the table structure
                using (var cmd = new NpgsqlCommand(@"
    INSERT INTO listings 
    (whoCreatedID, Name, Price, recommendedPrice, Description, isApproved, isActive, CreatedAt, UpdatedAt, ApprovedAt, DeletedAt, PCId, ImageUrls) 
    VALUES 
    (@whoCreatedID, @Name, @Price, @recommendedPrice, @Description, @isApproved, @isActive, @CreatedAt, @UpdatedAt, @ApprovedAt, @DeletedAt, @PCId, @ImageUrls)", connection))
                {
                    cmd.Parameters.AddWithValue("whoCreatedID", listing.whoCreatedid);
                    cmd.Parameters.AddWithValue("Name", listing.Name);
                    cmd.Parameters.AddWithValue("Price", listing.Price ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("recommendedPrice", listing.recommendedPrice ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("Description", listing.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("isApproved", listing.isApproved ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("isActive", listing.isActive);
                    cmd.Parameters.AddWithValue("CreatedAt", listing.CreatedAt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("UpdatedAt", listing.UpdatedAt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("ApprovedAt", listing.ApprovedAt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("DeletedAt", listing.DeletedAt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("PCId", listing.PCid ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("ImageUrls", listing.ImageUrls ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Listing inserted into the table.");
                }
                //using (var cmd = new NpgsqlCommand("INSERT INTO listings (title, price, discountPrice, description, isDiscounted, isApproved, createdDate, updatedDate, deletedDate, lastPurchaseDate, whoCreatedID, tags, stock) VALUES (@title, @price, @discountPrice, @description, @isDiscounted, @isApproved, @createdDate, @updatedDate, @deletedDate, @lastPurchaseDate, @whoCreatedID, @tags, @stock)", connection))
                //{
                //    cmd.Parameters.AddWithValue("title", NpgsqlDbType.Char , listing.Name);
                //    cmd.Parameters.AddWithValue("price", NpgsqlDbType.Integer ,listing.Price);
                //    cmd.Parameters.AddWithValue("description", NpgsqlDbType.Char, listing.Description);
                //    cmd.Parameters.AddWithValue("isActive", NpgsqlDbType.Boolean, listing.isActive);
                //    cmd.Parameters.AddWithValue("isApproved", NpgsqlDbType.Boolean, listing.isApproved);
                //    cmd.Parameters.AddWithValue("createdDate", NpgsqlDbType.Date, listing.CreatedAt);
                //    cmd.Parameters.AddWithValue("updatedDate", NpgsqlDbType.Date, listing.UpdatedAt);
                //    cmd.Parameters.AddWithValue("deletedDate", NpgsqlDbType.Date, listing.DeletedAt);
                //    cmd.Parameters.AddWithValue("whoCreatedID", NpgsqlDbType.Integer, listing.whoCreatedID);
                //
                //    cmd.ExecuteNonQuery();
                //}
            }
        }

        public List<Listing> FetchListings(int startIndex, int endIndex)
        {
            // Fetch listings from the database within a specified range using Npgsql
            List<Listing> listings = new List<Listing>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int limit = endIndex - startIndex + 1;
                using (var cmd = new NpgsqlCommand("SELECT * FROM listings OFFSET @startIndex LIMIT @limit", connection))
                {
                    cmd.Parameters.AddWithValue("startIndex", startIndex);
                    cmd.Parameters.AddWithValue("limit", limit);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Listing listing = new Listing
                            {
                                id = reader.GetInt32(0), // ID için doğru kolon
                                whoCreatedid = reader.GetInt32(1),
                                Name = reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                recommendedPrice = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                Description = reader.GetString(5),
                                isApproved = reader.GetBoolean(6),
                                isActive = reader.GetBoolean(7),
                                CreatedAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                                UpdatedAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                                ApprovedAt = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                                DeletedAt = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11),
                                PCid = reader.GetInt32(12),
                                ImageUrls = reader.IsDBNull(13) ? null : (string[])reader.GetValue(13)
                            };


                            listings.Add(listing);
                        }
                    }
                }
            }

            return listings;
        }

        public void GetAllComputers()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM Computers", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["Id"]);
                            Console.WriteLine(reader["Name"]);
                            Console.WriteLine(reader["Price"]);
                            Console.WriteLine(reader["Brand"]);
                            Console.WriteLine(reader["Model"]);
                            Console.WriteLine(reader["Warranty"]);
                            Console.WriteLine(reader["Stock"]);
                            Console.WriteLine(reader["Image"]);
                            Console.WriteLine(reader["Link"]);
                        }
                    }
                }
            }
        }


        public void InsertDataToPostgre<T>(List<T> dataList)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string tableName = typeof(T).Name.ToLower();

            if (typeof(T).Name.ToLower() == "powersupply")
                tableName = "powersupplie";


            CreateTableIfNotExists(connection, tableName, typeof(T));

            foreach (var item in dataList)
            {
                string columns = string.Join(", ", typeof(T).GetProperties().Select(p => p.Name.ToLower(new("en-US"))));
                string values = string.Join(", ", typeof(T).GetProperties().Select(p => $"@{p.Name}"));

                Console.WriteLine(columns + " - " + values);

                string query = $"INSERT INTO {tableName.ToLower()}s ({columns}) VALUES ({values})";

                using var command = new NpgsqlCommand(query, connection);
                foreach (var prop in typeof(T).GetProperties())
                {
                    var value = prop.GetValue(item) ?? DBNull.Value;
                    command.Parameters.AddWithValue($"@{prop.Name}", value);
                }

                Console.WriteLine("\n\n " + JsonConvert.SerializeObject(command.Parameters, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) + "\n\n");

                if (item is PC pc)
                {
                    Console.WriteLine(pc.id);
                }
                else if (item is Listing listing2)
                {
                    Console.WriteLine(listing2.Name);
                }

                try
                {
                    command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                    Console.WriteLine(JsonConvert.SerializeObject(item));
                    continue;
                }
            }
        }

        public static void CreateTableIfNotExists(NpgsqlConnection connection, string tableName, Type type)
        {
            string columns = string.Join(", ", type.GetProperties().Select(p =>
            {
                string columnName = p.Name.ToLower();
                string columnType = GetPostgreSQLType(p.PropertyType);
                return $"{columnName} {columnType}";
            }));

            string query = $@"
                CREATE TABLE IF NOT EXISTS {tableName}s (
                    id SERIAL PRIMARY KEY,
                    {columns}
                )";

            using var command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();
        }

        public static string GetPostgreSQLType(Type type)
        {
            if (type == typeof(string)) return "TEXT";
            if (type == typeof(int) || type == typeof(int?)) return "INTEGER";
            if (type == typeof(double) || type == typeof(double?)) return "DOUBLE PRECISION";
            if (type == typeof(decimal) || type == typeof(decimal?)) return "NUMERIC";
            if (type == typeof(bool) || type == typeof(bool?)) return "BOOLEAN";
            if (type == typeof(int[])) return "INTEGER[]";
            if (type == typeof(double[])) return "DOUBLE PRECISION[]";
            return "TEXT";
        }

        public User FetchUserFromDatabase(string userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM users WHERE id = @userId", connection))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            (
                                Id: reader.GetInt32(reader.GetOrdinal("Id")),
                                email: reader.GetString(reader.GetOrdinal("Email")),
                                password: reader.GetString(reader.GetOrdinal("Password")),
                                name: reader.GetString(reader.GetOrdinal("Name")),
                                role: reader.GetString(reader.GetOrdinal("Role"))
                           );
                        }
                    }
                }
            }

            return new User();
        }

        public User FetchUserFromDatabase(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM users WHERE id = @userId", connection))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            (
                                Id: reader.GetInt32(reader.GetOrdinal("Id")),
                                email: reader.GetString(reader.GetOrdinal("Email")),
                                password: reader.GetString(reader.GetOrdinal("Password")),
                                name: reader.GetString(reader.GetOrdinal("Name")),
                                role: reader.GetString(reader.GetOrdinal("Role"))
                            );
                        }
                    }
                }
            }
            return new User();
        }


        public User CheckIfRealUser(string email, string password)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM users WHERE email = @email AND password = @password", connection))
                {
                    command.Parameters.AddWithValue("email", email);
                    command.Parameters.AddWithValue("password", password);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            (
                                Id: reader.GetInt32(reader.GetOrdinal("Id")),
                                email: reader.GetString(reader.GetOrdinal("Email")),
                                password: reader.GetString(reader.GetOrdinal("Password")),
                                name: reader.GetString(reader.GetOrdinal("Name")),
                                role: reader.GetString(reader.GetOrdinal("Role"))
                           );
                        }
                    }
                }
            }

            return new User();
        }
    }
}
