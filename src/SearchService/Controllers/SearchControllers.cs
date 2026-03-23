using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchControllers : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams) //FromQuery để lấy dữ liệu từ query string của URL, giúp ánh xạ các tham số truy vấn vào đối tượng searchParams một cách tự động.
    {
        var query = DB.PagedSearch<Item, Item>(); // item 1 là kiểu dữ liệu của database, item 2 là kiểu dữ liệu của kết quả trả về. Nếu muốn trả về một kiểu khác, có thể thay đổi item 2 thành kiểu mong muốn.
        query.Sort(x => x.Ascending(a => a.Make));

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endsoon" => query.Match(x => x.AuctionEnd > DateTime.UtcNow && x.AuctionEnd < DateTime.UtcNow.AddHours(6)),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow),
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);
        var result = await query.ExecuteAsync();
        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }


}
