namespace Tool.Compet.EntityFrameworkCore;

using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

// Extension of IQueryable<T>
// Where T: item type
public static class PaginationExt {
	/// Note: given `pageIndex * pageSize` must be in range of Int32.
	/// @param pagePos: Index (from 1) of the page. For eg,. 1, 2, 3,...
	/// @param pageSize: Item count in the page. For eg,. 10, 20, 50,...
	public static async Task<PagedResult<T>> PaginateAsyncDk<T>(
		this IQueryable<T> query,
		int page,
		int limit
	) where T : class {
		// Offset equals to item-count as far to now. For eg,. offset is 50 when query at page 2 with item 50.
		// TechNote: use max to prevent negative index from overflow
		var offset = Math.Max(0, (page - 1) * limit);

		// Number of all items of the query.
		var totalItemCount = await query.CountAsync();

		// Query and take some items in range [offset, offset + pageSize - 1]
		var items = await query.Skip(offset).Take(limit).ToArrayAsync();

		// Number of page.
		// This calculation is faster than `Math.Ceiling(rowCount / pageSize)`
		var pageCount = (totalItemCount + limit - 1) / limit;

		return new PagedResult<T>(
			items: items,
			pager: new(page, pageCount, totalItemCount)
		);
	}

	/// <summary>
	/// Note: given `pageIndex * pageSize` must be in range of Int32.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="query"></param>
	/// <param name="leftPaddingItems">Items which be added at left of query result.</param>
	/// <param name="page">Index (from 1) of the page. For eg,. 1, 2, 3,...</param>
	/// <param name="limit">Item count in the page. For eg,. 10, 20, 50,...</param>
	/// <returns></returns>
	public static async Task<PagedResult<T>> PaginateAsyncDk<T>(
		this IQueryable<T> query,
		IEnumerable<T> leftPaddingItems,
		int page,
		int limit
	) where T : class {
		// Offset equals to item-count as far to now. For eg,. offset is 50 when query at page 2 with item 50.
		// TechNote: use max to prevent negative index from overflow
		var offset = Math.Max(0, (page - 1) * limit);

		var leftPaddingItemCount = leftPaddingItems.Count();
		var totalItemCount = leftPaddingItemCount + (await query.CountAsync());

		// Query and take some items in range [offset, offset + pageSize - 1]
		var items = new List<T>();
		var takeCount1 = Math.Min(leftPaddingItemCount - offset, limit);
		if (takeCount1 > 0) {
			items.AddRange(leftPaddingItems.Skip(offset).Take(takeCount1));
		}
		var takeCount2 = limit - items.Count;
		if (takeCount2 > 0) {
			var skipCount = Math.Max(0, offset - leftPaddingItemCount);
			items.AddRange(await query.Skip(skipCount).Take(takeCount2).ToArrayAsync());
		}

		// This calculation is faster than `Math.Ceiling(rowCount / pageSize)`
		var pageCount = (totalItemCount + limit - 1) / limit;

		return new PagedResult<T>(
			items: [.. items],
			pager: new(page, pageCount, totalItemCount)
		);
	}
}

public class PagedResult<T>(T[] items, Pager pager) where T : class {
	/// Items in the page.
	/// Note: can use `IEnumerable<T>` for more abstract that can cover both of array and list.
	public readonly T[] items = items;

	public readonly Pager pager = pager;
}

public class Pager(int page, int count, int total) {
	/// Page position (1-based index)
	[JsonPropertyName("page")]
	public int page { get; set; } = page;

	/// Page count
	[JsonPropertyName("count")]
	public int count { get; set; } = count;

	/// Total item count
	[JsonPropertyName("total")]
	public int total { get; set; } = total;
}
