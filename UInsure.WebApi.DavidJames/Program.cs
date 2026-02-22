using UInsure.WebApi.DavidJames.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using UInsure.WebApi.DavidJames.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAutoMapper(typeof(Program));

        // Setup our in-memory DB
        builder.Services.AddDbContext<UinsureDbContext>(options =>
            options.UseInMemoryDatabase("UinsureDb"));

        // Setup DI
        builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
        builder.Services.AddScoped<IPolicyService, PolicyService>();

        var app = builder.Build();

        // We could always make this available in prod to help the consuming devs if they are external, one for later.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // This will give us security to hide system errors. We want to expose information about what the consumer may have done
        // incorrectly. Not our call stacks!  Business errors are fine, we don't want to expose our stack to... people who
        // may not have our best interests at heart! During development, it is helpful to have the full stack.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/problem+json";

                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Title = "An unexpected error occurred.",
                        Status = 500
                    });
                });
            });
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}