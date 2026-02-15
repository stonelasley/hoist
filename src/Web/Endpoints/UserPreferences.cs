using Hoist.Application.UserPreferences.Commands.UpsertUserPreferences;
using Hoist.Application.UserPreferences.Queries.GetUserPreferences;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hoist.Web.Endpoints;

public class UserPreferences : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetUserPreferences).RequireAuthorization();
        groupBuilder.MapPut(UpsertUserPreferences, string.Empty).RequireAuthorization();
    }

    public async Task<Ok<UserPreferencesDto>> GetUserPreferences(ISender sender)
    {
        var result = await sender.Send(new GetUserPreferencesQuery());

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> UpsertUserPreferences(ISender sender, UpsertUserPreferencesCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
}
