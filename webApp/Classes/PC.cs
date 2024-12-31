using Npgsql;

namespace webApp.Classes
{
    public class PC
    {
        public int id { get; set; }
        private static int _counter = 100000;
        public int motherboardId { get; set; }
        public int[] coolerIds { get; set; }
        public int cpuId { get; set; }
        public int ramId { get; set; }
        public int gpuId { get; set; }
        public int[] storageIds { get; set; }
        public int powerSupplyId { get; set; }
        public int caseId { get; set; }



        private const string ConnectionString = "Host=localhost;Port=5432;Username=mursel;Password=naberabla121224;Database=YazilimMuhProject";

        public static PC GenerateRandomPC()
        {
            var pc = new PC();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                pc.id = _counter++;
                pc.motherboardId = GetRandomId(connection, "motherboards");
                pc.coolerIds = new int[] { GetRandomId(connection, "coolers"), GetRandomId(connection, "coolers") };
                pc.cpuId = GetRandomId(connection, "cpus");
                pc.ramId = GetRandomId(connection, "rams");
                pc.gpuId = GetRandomId(connection, "gpus");
                pc.storageIds = new int[] { GetRandomId(connection, "storages"), GetRandomId(connection, "storages") };
                pc.powerSupplyId = GetRandomId(connection, "powersupplies");
                pc.caseId = GetRandomId(connection, "cases");

                connection.Close();
            }

            return pc;
        }

        private static int GetRandomId(NpgsqlConnection connection, string tableName)
        {
            string query = $"SELECT id FROM {tableName} ORDER BY RANDOM() LIMIT 1";

            using (var command = new NpgsqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }



}
