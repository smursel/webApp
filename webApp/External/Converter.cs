using Npgsql;
using System;
using System.Collections.Generic;
using webApp.Classes;

public class ListingConverter
{
	private readonly string _connectionString;

	public ListingConverter(string connectionString)
	{
		_connectionString = connectionString;
	}

	public VisualListing ConvertToVisualListing(Listing listing)
	{
		if (listing == null) throw new ArgumentNullException(nameof(listing));

		var visualListing = new VisualListing
		{
			id = listing.id,
			Name = listing.Name,
			Price = listing.Price,
			Description = listing.Description,
			ImageUrls = listing.ImageUrls ?? Array.Empty<string>(),
			GpuName = GetComponentNameById("gpus", listing.PCid, "gpuId"),
			CpuName = GetComponentNameById("cpus", listing.PCid, "cpuId"),
			MotherboardName = GetComponentNameById("motherboards", listing.PCid, "motherboardId"),
			storageName = GetComponentNamesByArray("storages", listing.PCid, "storageIds"),
			CaseName = GetComponentNameById("cases", listing.PCid, "caseId"),
			PowerSupplyName = GetComponentNameById("powersupplies", listing.PCid, "powerSupplyId"),
			coolerNames = GetComponentNamesByArray("coolers",listing.PCid,"coolerids"),
			RamName = GetComponentNameById("rams", listing.PCid, "ramId"),
			isApproved = listing.isApproved.Value,
			isActive = listing.isActive
		};

		return visualListing;
	}

    public VisualListing ConvertToVisualListing(Listing listing,bool gpuModel)
    {
        if (listing == null) throw new ArgumentNullException(nameof(listing));

        var visualListing = new VisualListing
        {
            id = listing.id,
            Name = listing.Name,
            Price = listing.Price,
            Description = listing.Description,
            ImageUrls = listing.ImageUrls ?? Array.Empty<string>(),
            GpuName = GetComponentNameById("gpus", listing.PCid, "gpuId", gpuModel),
            CpuName = GetComponentNameById("cpus", listing.PCid, "cpuId"),
            MotherboardName = GetComponentNameById("motherboards", listing.PCid, "motherboardId"),
            storageName = GetComponentNamesByArray("storages", listing.PCid, "storageIds"),
            CaseName = GetComponentNameById("cases", listing.PCid, "caseId"),
            PowerSupplyName = GetComponentNameById("powersupplies", listing.PCid, "powerSupplyId"),
            coolerNames = GetComponentNamesByArray("coolers", listing.PCid, "coolerids"),
            RamName = GetComponentNameById("rams", listing.PCid, "ramId"),
            isApproved = listing.isApproved.Value,
            isActive = listing.isActive
        };

        return visualListing;
    }

    private List<string> GetComponentNamesByArray(string tableName, int? pcId, string columnName)
    {
        if (!pcId.HasValue) return new List<string> { "N/A" };

        var query = $@"
        SELECT name 
        FROM {tableName}
        WHERE id = ANY (
            SELECT UNNEST({columnName}) 
            FROM pcs 
            WHERE id = @PcId
        )";

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@PcId", pcId);

        connection.Open();
        var result = new List<string>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                result.Add(reader.GetString(0));
            }
        }
        connection.Close();

        return result.Count > 0 ? result : new List<string> { "N/A" };
    }

    private string GetComponentNameById(string tableName, int? pcId, string columnName,bool isModel = false)
	{
		if (!pcId.HasValue) return "N/A";

		
		var query = $"SELECT name FROM {tableName} WHERE id = (SELECT {columnName} FROM pcs WHERE id = @PcId)";
        if (isModel)
        {
             query = $"SELECT chipset FROM {tableName} WHERE id = (SELECT {columnName} FROM pcs WHERE id = @PcId)";
        }

        using var connection = new NpgsqlConnection(_connectionString);
		using var command = new NpgsqlCommand(query, connection);
		command.Parameters.AddWithValue("@PcId", pcId);

		connection.Open();
		var result = command.ExecuteScalar();
		connection.Close();

		return result?.ToString() ?? "N/A";
	}

	private string[] GetRamNames(int? pcId)
	{
		if (!pcId.HasValue) return Array.Empty<string>();

		var query = @"SELECT name FROM rams WHERE id = ANY(
                        SELECT ramid FROM pcs WHERE id = @PcId)";
		using var connection = new NpgsqlConnection(_connectionString);
		using var command = new NpgsqlCommand(query, connection);
		command.Parameters.AddWithValue("@PcId", pcId);

		connection.Open();
		var ramNames = new List<string>();
		using var reader = command.ExecuteReader();
		while (reader.Read())
		{
			ramNames.Add(reader.GetString(0));
		}
		connection.Close();

		return ramNames.ToArray();
	}

	private List<string> GetCoolerNames(int? pcId)
	{
		if (!pcId.HasValue) return new List<string>();

		var query = @"SELECT name FROM coolers WHERE id = ANY(
                        SELECT coolerids FROM pcs WHERE id = @PcId)";
		using var connection = new NpgsqlConnection(_connectionString);
		using var command = new NpgsqlCommand(query, connection);
		command.Parameters.AddWithValue("@PcId", pcId);

		connection.Open();
		var coolerNames = new List<string>();
		using var reader = command.ExecuteReader();
		while (reader.Read())
		{
			coolerNames.Add(reader.GetString(0));
		}
		connection.Close();

		return coolerNames;
	}
}
