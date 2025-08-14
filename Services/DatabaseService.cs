using Npgsql;
using HarborMaster.Models;
using System.Data;

namespace HarborMaster.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _schemaFilePath = "database-setup.sql";

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HarborMasterConnectionString") ??
                throw new InvalidOperationException("Connection string 'HarborMasterConnectionString' not found.");
            // Optionally, you can make _schemaFilePath configurable if needed
        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Helper method to execute non-query SQL commands
        public async Task ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            await command.ExecuteNonQueryAsync();
        }

        // Method to initialize the database: creates DB if missing, then sets up tables
        public async Task InitializeDatabaseAsync()
        {
            // Connect to Postgres system DB to check/create harbormaster DB
            var sysConnStr = _connectionString.Replace("Database=harbormaster", "Database=postgres");
            using (var connection = new NpgsqlConnection(sysConnStr))
            {
                await connection.OpenAsync();
                using var checkCommand = new NpgsqlCommand(
                    "SELECT 1 FROM pg_database WHERE datname = 'harbormaster'", connection);
                var exists = await checkCommand.ExecuteScalarAsync();
                if (exists == null)
                {
                    using var createDbCommand = new NpgsqlCommand(
                        "CREATE DATABASE harbormaster", connection);
                    await createDbCommand.ExecuteNonQueryAsync();
                }
            }

            // Connect to harbormaster DB and run schema setup
            if (!File.Exists(_schemaFilePath))
                throw new FileNotFoundException($"Schema file not found: {_schemaFilePath}");

            string sql = await File.ReadAllTextAsync(_schemaFilePath);
            await ExecuteNonQueryAsync(sql);
        }
        public async Task SeedDatabaseAsync()
        {
            // Check if data already exists
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Check if docks table has data
            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM docks", connection);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (count > 0)
            {
                // Data already exists, no need to seed
                return;
            }

            // Seed docks
            await ExecuteNonQueryAsync(@"
        INSERT INTO docks (location, capacity) VALUES
        ('North Harbor', 5),
        ('South Harbor', 3),
        ('East Harbor', 7)
    ");

            // Seed haulers
            await ExecuteNonQueryAsync(@"
        INSERT INTO haulers (name, capacity) VALUES
        ('Oceanic Haulers', 10),
        ('Maritime Transport', 15),
        ('Sea Logistics', 8)
    ");

            // Seed ships
            await ExecuteNonQueryAsync(@"
        INSERT INTO ships (name, type, dock_id) VALUES
        ('Serenity', 'Firefly-class transport ship', 1),
        ('Rocinante', 'Corvette-class frigate', 2),
        ('Millennium Falcon', 'YT-1300 light freighter', 3),
        ('Black Pearl', 'Pirate galleon', 1),
        ('Nautilus', 'Submarine vessel', 2),
        ('Flying Dutchman', 'Ghost ship', 3),
        ('Enterprise', 'Constitution-class starship', 1),
        ('Voyager', 'Intrepid-class starship', 2),
        ('Defiant', 'Escort-class warship', 3),
        ('Galactica', 'Battlestar', 1),
        ('Bebop', 'Fishing trawler', 2),
        ('Normandy', 'Stealth frigate', 3),
        ('Pillar of Autumn', 'Halcyon-class cruiser', 1),
        ('Nostromo', 'Commercial towing vessel', 2),
        ('Sulaco', 'Military transport', 3),
        ('Highwind', 'Airship', 1),
        ('Argo', 'Ancient Greek galley', 2),
        ('Nebuchadnezzar', 'Hovership', 3)
    ");
        }

    }
}