using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task DbInit(WebApplication app)
    {
        //kết nối đến MongoDB
        await DB.InitAsync("SearchDB", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDBConnection")));

        //viết index
        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();

        //lấy dữ liệu từ Auction Service
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSerchDB();
        Console.WriteLine(items.Count + " items fetched from Auction Service");
        if (items.Count > 0) await DB.SaveAsync(items);
    }
}
