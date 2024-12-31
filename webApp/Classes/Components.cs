using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;


namespace webApp.Classes
{
    // ankarakart asdasdasd
    public class Motherboard
    {
        private static int _counter = 100000;
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Socket { get; set; }

        [JsonProperty("form_factor")]
        public string FormFactor { get; set; }
        public int MaxMemory { get; set; }

        [JsonProperty("memory_slots")]
        public int MemorySlots { get; set; }
        public string Color { get; set; }

        public Motherboard()
        {
            Id = _counter++;
        }
    }

    public class Cooler
    {
        private static int _counter = 100000;
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public List<int?> Rpm { get; set; }
        public List<double?> NoiseLevel { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }

        public Cooler()
        {
            Id = _counter++;
        }

        public static Cooler CoolerSerializer(JObject json)
        {
            var cooler = new Cooler
            {
                Name = json["name"]?.ToString(),
                Price = json["price"]?.ToObject<decimal?>(),
                Color = json["color"]?.ToString(),
                Size = json["size"]?.ToString()
            };

            // Rpm (tekil veya dizi olarak gelebilir)
            if (json["rpm"] is JArray rpmArray)
            {
                cooler.Rpm = rpmArray.ToObject<List<int?>>();
            }
            else if (json["rpm"] != null)
            {
                cooler.Rpm = new List<int?> { json["rpm"].ToObject<int?>() };
            }


            // NoiseLevel (tekil veya dizi olarak gelebilir)
            if (json["noise_level"] is JArray noiseArray)
            {
                cooler.NoiseLevel = noiseArray.ToObject<List<double?>>();
            }
            else if (json["noise_level"] != null)
            {
                cooler.NoiseLevel = new List<double?> { json["noise_level"].ToObject<double?>() };
            }


            return cooler;
        }

        public static List<Cooler> CoolerSerializerList(JArray jsonArray)
        {
            var coolers = new List<Cooler>();

            foreach (var item in jsonArray)
            {
                if (item is JObject jsonObject)
                {
                    coolers.Add(CoolerSerializer(jsonObject));
                }
            }

            return coolers;
        }

    }

    // İşlemci sınıfı
    public class CPU
    {
        private static int _counter = 100000;
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        [JsonProperty("core_count")]
        public int? CoreCount { get; set; }
        [JsonProperty("core_clock")]
        public double? CoreClock { get; set; }
        [JsonProperty("boost_clock")]
        public double? BoostClock { get; set; }
        public int TDP { get; set; }
        public string Graphics { get; set; }
        public bool SMT { get; set; }

        public CPU()
        {
            Id = _counter++;

            Console.WriteLine("myName: " + Name + " - " + CoreCount);
        }
    }

    // guc kaynagı
    public class PowerSupply
    {
        private static int _counter = 100000;
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Type { get; set; }
        public string Efficiency { get; set; }
        public int Wattage { get; set; }
        public string Modular { get; set; }
        public string Color { get; set; }

        public PowerSupply()
        {
            Id = _counter++;
        }

        public static PowerSupply PowerSupplySerializer(JObject json)
        {
            var powerSupply = new PowerSupply
            {
                Name = json["name"]?.ToString(),
                Price = json["price"]?.ToObject<decimal?>(),
                Type = json["type"]?.ToString(),
                Efficiency = json["efficiency"]?.ToString(),
                Wattage = json["wattage"]?.ToObject<int>() ?? 0,
                Color = json["color"]?.ToString() ?? "unknown"
            };

            string text = json["modular"].ToString();
            powerSupply.Modular = text;

            return powerSupply;
        }

        public static List<PowerSupply> PowerSupplySerializerList(JArray jsonArray)
        {
            var powerSupplies = new List<PowerSupply>();

            foreach (var item in jsonArray)
            {
                if (item is JObject jsonObject)
                {
                    powerSupplies.Add(PowerSupplySerializer(jsonObject));
                }
            }

            return powerSupplies;
        }
    }

    // ekran kart
    public class GPU
    {
        private static int _counter = 100000;
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Chipset { get; set; }
        public double Memory { get; set; }
        
        [JsonProperty("core_clock")]
        public int? CoreClock { get; set; }

        [JsonProperty("boost_clock")]
        public int? BoostClock { get; set; }
        public string Color { get; set; }
        public int? Length { get; set; }

        public GPU()
        {
            Id = _counter++;
        }
    }

    public class Case
    {
        public int Id { get; set; }
        private static int _counter = 1;
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public string PSU { get; set; }
        public string SidePanel { get; set; }
        public double ExternalVolume { get; set; }
        public int Internal35Bays { get; set; }

        public Case()
        {
            Id = _counter;
            _counter++;
        }
    }

    public class Ram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }
        public List<int?> Speed { get; set; }
        public List<int?> Modules { get; set; }
        public double? PricePerGb { get; set; }
        public string Color { get; set; }
        public double? FirstWordLatency { get; set; }
        public int CasLatency { get; set; }

        public static Ram RamSerializer(JObject json)
        {
            var ram = new Ram
            {
                Name = json["name"]?.ToString(),
                Price = json["price"]?.ToObject<double?>() ?? 0.0,
                PricePerGb = json["price_per_gb"]?.ToObject<double?>() ?? 0.0, // Değişiklik burada
                Color = json["color"]?.ToString() ?? "unknown",
                FirstWordLatency = json["first_word_latency"]?.ToObject<double?>() ?? 0.0, // Değişiklik burada
                CasLatency = json["cas_latency"]?.ToObject<int>() ?? 0
            };

            // Speed (tekil veya dizi olarak gelebilir)
            if (json["speed"] is JArray speedArray)
            {
                ram.Speed = speedArray.ToObject<List<int?>>();
            }
            else if (json["speed"] != null)
            {
                ram.Speed = new List<int?> { json["speed"].ToObject<int?>() };
            }

            // Modules (tekil veya dizi olarak gelebilir)
            if (json["modules"] is JArray modulesArray)
            {
                ram.Modules = modulesArray.ToObject<List<int?>>();
            }
            else if (json["modules"] != null)
            {
                ram.Modules = new List<int?> { json["modules"].ToObject<int?>() };
            }

            return ram;
        }

        public static List<Ram> RamSerializerList(JArray jsonArray)
        {
            var rams = new List<Ram>();

            foreach (var item in jsonArray)
            {
                if (item is JObject jsonObject)
                {
                    rams.Add(RamSerializer(jsonObject));
                }
            }

            return rams;
        }
    }

    public class Storage
    {
        public int Id { get; set; }
        private static int _counter = 100000;
        public string Name { get; set; }
        public double? Price { get; set; }
        public double Capacity { get; set; }
        public double PricePerGb { get; set; }
        public string Type { get; set; }
        public string Cache { get; set; }
        public string FormFactor { get; set; }
        public string Interface { get; set; }

        public Storage()
        {
            Id = _counter++;
        }
    }
}
