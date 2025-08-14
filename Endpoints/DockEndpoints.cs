using HarborMaster.Models;
using HarborMaster.Services;
namespace HarborMaster.Endpoints
{
    public static class DockEndpoints
    {
        /*
          This method becomes an extension method of the app
          defined in Program.cs by using the `this` keyword
       */
        public static void MapDockEndpoints(this WebApplication app)
        {
            app.MapGet("/docks", async (DatabaseService db) =>
                await db.GetAllDocksAsync());

            app.MapGet("/docks/{id}", async (int id, DatabaseService db) =>
            {
                var dock = await db.GetDockByIdAsync(id);
                return dock != null ? Results.Ok(dock) : Results.NotFound();
            });
        }
    }
}