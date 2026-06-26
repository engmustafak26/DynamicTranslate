using DynamicTranslate.Demo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DynamicTranslate;
using DynamicTranslate.Demo.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDynamicTranslate(typeof(ApplicationDbContext)); //register dynamic translation services

builder.Services.AddControllers(opt=>opt.Filters.Add<TranslationResultFilter>()); // register translation filter
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
