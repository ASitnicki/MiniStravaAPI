using Microsoft.Data.SqlClient;
using System.Text;

namespace MiniStrava.Utils
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IConfiguration config, ILogger logger)
        {
            var cs = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Missing connection string: DefaultConnection");

            var csb = new SqlConnectionStringBuilder(cs);
            var dbName = csb.InitialCatalog;

            // 1) poczekaj aż SQL Server wstanie + utwórz DB (master)
            var masterCsb = new SqlConnectionStringBuilder(cs) { InitialCatalog = "master" };

            const int maxAttempts = 30;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await using var masterConn = new SqlConnection(masterCsb.ConnectionString);
                    await masterConn.OpenAsync();

                    await using var cmd = masterConn.CreateCommand();
                    cmd.CommandText = $"IF DB_ID(N'{dbName}') IS NULL CREATE DATABASE [{dbName}];";
                    await cmd.ExecuteNonQueryAsync();

                    break;
                }
                catch (SqlException ex)
                {
                    logger.LogWarning(ex, "DB not ready yet (attempt {Attempt}/{Max})", attempt, maxAttempts);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            // 2) jeśli nie ma tabeli Users -> odpal schema.sql
            await using var dbConn = new SqlConnection(csb.ConnectionString);
            await dbConn.OpenAsync();

            bool usersExists;
            await using (var check = dbConn.CreateCommand())
            {
                check.CommandText = "SELECT CASE WHEN OBJECT_ID(N'[Users]', N'U') IS NULL THEN 0 ELSE 1 END;";
                usersExists = (int)(await check.ExecuteScalarAsync()) == 1;
            }

            if (usersExists)
            {
                logger.LogInformation("Database schema already exists (Users table found).");
                return;
            }

            // schema.sql w kontenerze: /app/db-init/schema.sql (albo lokalnie: ./db-init/schema.sql)
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "db-init", "schema.sql");
            if (!File.Exists(scriptPath))
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "db-init", "schema.sql");

            if (!File.Exists(scriptPath))
                throw new FileNotFoundException("schema.sql not found. Ensure db-init/schema.sql is copied/mounted into API container.", scriptPath);

            var script = await File.ReadAllTextAsync(scriptPath, Encoding.UTF8);

            await using (var cmd = dbConn.CreateCommand())
            {
                cmd.CommandText = script;
                cmd.CommandTimeout = 180;
                await cmd.ExecuteNonQueryAsync();
            }

            logger.LogInformation("Database initialized using schema.sql.");
        }
    }
}
