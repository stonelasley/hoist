using System.Text;
using System.Text.Json;
using Hoist.Application.Common.Interfaces;
using Hoist.Application.Workouts.Queries.GetRecentWorkouts;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Queries.GetWorkoutHistory;

public record GetWorkoutHistoryQuery : IRequest<PaginatedWorkoutList>
{
    public string? SortBy { get; init; }  // "date" (default) or "rating"
    public string? SortDirection { get; init; }  // "desc" (default) or "asc"
    public int? LocationId { get; init; }
    public int? MinRating { get; init; }
    public string? Search { get; init; }  // search in Notes
    public string? Cursor { get; init; }  // Base64-encoded cursor
    public int PageSize { get; init; } = 20;
}

public class GetWorkoutHistoryQueryHandler : IRequestHandler<GetWorkoutHistoryQuery, PaginatedWorkoutList>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _currentUser;

    public GetWorkoutHistoryQueryHandler(IApplicationDbContext context, IMapper mapper, IUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PaginatedWorkoutList> Handle(GetWorkoutHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id ?? throw new UnauthorizedAccessException();

        // Start with completed workouts for current user
        var query = _context.Workouts
            .Where(w => w.UserId == userId && w.Status == WorkoutStatus.Completed);

        // Apply filters
        if (request.LocationId.HasValue)
        {
            query = query.Where(w => w.LocationId == request.LocationId.Value);
        }

        if (request.MinRating.HasValue)
        {
            query = query.Where(w => w.Rating >= request.MinRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(w => w.Notes != null && w.Notes.Contains(request.Search));
        }

        // Determine sort parameters
        var sortBy = request.SortBy?.ToLower() ?? "date";
        var sortDirection = request.SortDirection?.ToLower() ?? "desc";
        var isAscending = sortDirection == "asc";

        // Decode cursor if provided
        CursorData? cursorData = null;
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            try
            {
                var cursorJson = Encoding.UTF8.GetString(Convert.FromBase64String(request.Cursor));
                cursorData = JsonSerializer.Deserialize<CursorData>(cursorJson);
            }
            catch
            {
                // Invalid cursor - ignore and start from beginning
            }
        }

        // Apply cursor-based pagination
        if (cursorData != null)
        {
            if (sortBy == "rating")
            {
                // Sort by Rating, then by Id for stable sort
                if (isAscending)
                {
                    // For ascending: WHERE (Rating > cursor.Rating) OR (Rating = cursor.Rating AND Id > cursor.Id)
                    query = query.Where(w =>
                        (w.Rating > cursorData.Rating) ||
                        (w.Rating == cursorData.Rating && w.Id > cursorData.Id));
                }
                else
                {
                    // For descending: WHERE (Rating < cursor.Rating) OR (Rating = cursor.Rating AND Id > cursor.Id)
                    query = query.Where(w =>
                        (w.Rating < cursorData.Rating) ||
                        (w.Rating == cursorData.Rating && w.Id > cursorData.Id));
                }
            }
            else // date
            {
                // Sort by EndedAt, then by Id for stable sort
                if (isAscending)
                {
                    // For ascending: WHERE (EndedAt > cursor.EndedAt) OR (EndedAt = cursor.EndedAt AND Id > cursor.Id)
                    query = query.Where(w =>
                        (w.EndedAt > cursorData.EndedAt) ||
                        (w.EndedAt == cursorData.EndedAt && w.Id > cursorData.Id));
                }
                else
                {
                    // For descending: WHERE (EndedAt < cursor.EndedAt) OR (EndedAt = cursor.EndedAt AND Id > cursor.Id)
                    query = query.Where(w =>
                        (w.EndedAt < cursorData.EndedAt) ||
                        (w.EndedAt == cursorData.EndedAt && w.Id > cursorData.Id));
                }
            }
        }

        // Apply sorting
        if (sortBy == "rating")
        {
            query = isAscending
                ? query.OrderBy(w => w.Rating).ThenBy(w => w.Id)
                : query.OrderByDescending(w => w.Rating).ThenBy(w => w.Id);
        }
        else // date
        {
            query = isAscending
                ? query.OrderBy(w => w.EndedAt).ThenBy(w => w.Id)
                : query.OrderByDescending(w => w.EndedAt).ThenBy(w => w.Id);
        }

        // Take PageSize + 1 to determine if there's a next page
        var items = await query
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken);

        // Determine if there's a next page
        var hasMore = items.Count > request.PageSize;
        if (hasMore)
        {
            items = items.Take(request.PageSize).ToList();
        }

        // Create next cursor if there are more items
        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var lastItem = items[^1];
            var nextCursorData = new CursorData
            {
                EndedAt = lastItem.EndedAt,
                Rating = lastItem.Rating,
                Id = lastItem.Id
            };
            var cursorJson = JsonSerializer.Serialize(nextCursorData);
            nextCursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(cursorJson));
        }

        // Map to DTOs
        var dtos = _mapper.Map<List<WorkoutBriefDto>>(items);

        return new PaginatedWorkoutList
        {
            Items = dtos,
            NextCursor = nextCursor
        };
    }

    private class CursorData
    {
        public DateTimeOffset? EndedAt { get; set; }
        public int? Rating { get; set; }
        public int Id { get; set; }
    }
}
