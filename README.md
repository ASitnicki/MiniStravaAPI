# MiniStravaAPI

uruchamianie dockera: docker-compose up --build

using (var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection")))
{
    connection.Open();
    var script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "db-init/schema.sql"));
    using var command = new SqlCommand(script, connection);
    command.ExecuteNonQuery();
}