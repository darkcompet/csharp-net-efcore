namespace Tool.Compet.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

// Extension of IQueryable<T>
// Where T: item type
public static class PaginationExt {
	/// @param pagePos Position of the page. Index from 1 (NOT from 0).
	/// @param pageSize Page item count. Indicates how many items in each page (for eg,. 10, 20, 50,...).
	/// Note: given `pageIndex * pageSize` must be in range of Int32.
	public static async Task<PagedResult<T>> PaginateDk<T>(this IQueryable<T> query, int pagePos = 1, int pageSize = 50) where T : class {
		// Maybe zero- or overflow
		var offset = Math.Max(0, (pagePos - 1) * pageSize);

		// Use `CountAsync()` since we are using EF Core.
		// Use `Count()` will lead to weird result.
		var totalItemCount = await query.CountAsync();

		// Query and take some items in range [offset + 1, offset + pageSize]
		var items = await query.Skip(offset).Take(pageSize).ToArrayAsync();

		// This calculation is faster than `Math.Ceiling(rowCount / pageSize)`
		var pageCount = (totalItemCount + pageSize - 1) / pageSize;

		return new PagedResult<T>(
			items: items,
			pagePos: pagePos,
			pageCount: pageCount,
			totalItemCount: totalItemCount
		);
	}
}

public class PagedResult<T> where T : class {
	/// Item list in the page
	public readonly T[]? items;

	/// Position (1-index-based) of current page
	public readonly int pagePos;

	/// Total number of page
	public readonly int pageCount;

	/// Total item count
	public readonly int totalItemCount;

	public PagedResult(T[]? items, int pagePos, int pageCount, int totalItemCount) {
		this.items = items;
		this.pagePos = pagePos;
		this.pageCount = pageCount;
		this.totalItemCount = totalItemCount;
	}
}
