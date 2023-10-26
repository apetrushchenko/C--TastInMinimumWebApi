
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TasksDb>( options => 
    { 
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
    }
);

List < Tasks > tasks = new List< Tasks >();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
        var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<TasksDb>();
        db.Database.EnsureCreated(); 
}


app.MapGet("/tasks", async(TasksDb db ) => await db.tasks.ToListAsync()  );

app.MapGet("/tasks/{id}", async (int id, TasksDb db) => await db.tasks.FirstOrDefaultAsync(t => t.Id == id) is Tasks t2
? Results.Ok( t2 ) 
: Results.NotFound() );

app.MapPost("/tasks", async( [FromBody] Tasks task, [FromServices] TasksDb db) =>
{
    db.tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPut("/tasks", async ([FromBody] Tasks task, [FromServices] TasksDb db) =>
{
    var taskFromDB = await db.tasks.FindAsync(new object[] { task.Id });
    if (taskFromDB == null) return Results.NotFound();   
    taskFromDB.Name = task.Name;
    taskFromDB.Description = task.Description;
    taskFromDB.Status = task.Status;
    taskFromDB.Start  = task.Start;
    taskFromDB.Finish = task.Finish;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tasks/{id}", async (int id, TasksDb db) =>
{ 
    var taskFromDB = await db.tasks.FindAsync(new object[] { id });
    if ( taskFromDB == null ) return Results.NotFound();
    db.tasks.Remove(taskFromDB);
    await db.SaveChangesAsync();
    return Results.NoContent();
} );

app.Run();


public class TasksDb : DbContext
{
    public TasksDb(DbContextOptions<TasksDb> options) : base(options) { }

    public DbSet<Tasks> tasks => Set<Tasks>();   
}

public class Tasks
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    Status { get; set; } = 0;
    public string Start { get; set; } = DateTime.MinValue.ToString();
    public string Finish { get; set; } = DateTime.MinValue.ToString();
}