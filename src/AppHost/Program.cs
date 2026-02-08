var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("HoistDb");

builder.AddProject<Projects.Web>("web")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
