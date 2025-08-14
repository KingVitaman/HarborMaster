using HarborMaster.Models;
using HarborMaster.Services;

namespace HarborMaster.Endpoints
{
    public static class ShipEndpoints
    {
        /*
           This method becomes an extension method of the app
           defined in Program.cs by using the `this` keyword
        */
        public static void MapShipEndpoints(this WebApplication app)
        {
            // GET /ships - Get all ships
            app.MapGet("/ships", async (DatabaseService db) =>
                await db.GetAllShipsAsync());

            // GET /ships/{id} - Get a ship by ID
            app.MapGet("/ships/{id}", async (int id, DatabaseService db) =>
            {
                var ship = await db.GetShipByIdAsync(id);
                return ship != null ? Results.Ok(ship) : Results.NotFound();
            });
        }
    }
}