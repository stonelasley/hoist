using Hoist.Application.Locations.Commands.CreateLocation;
using Hoist.Application.Locations.Commands.DeleteLocation;
using Hoist.Application.Locations.Commands.UpdateLocation;
using Hoist.Application.Locations.Queries.GetLocation;
using Hoist.Application.Locations.Queries.GetLocations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hoist.Web.Endpoints;

public class Locations : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetLocations).RequireAuthorization();
        groupBuilder.MapGet(GetLocation, "{id}").RequireAuthorization();
        groupBuilder.MapPost(CreateLocation).RequireAuthorization();
        groupBuilder.MapPut(UpdateLocation, "{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteLocation, "{id}").RequireAuthorization();
    }

    public async Task<Ok<List<LocationBriefDto>>> GetLocations(ISender sender)
    {
        var result = await sender.Send(new GetLocationsQuery());

        return TypedResults.Ok(result);
    }

    public async Task<Ok<LocationBriefDto>> GetLocation(ISender sender, int id)
    {
        var result = await sender.Send(new GetLocationQuery(id));

        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateLocation(ISender sender, CreateLocationCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Locations)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateLocation(ISender sender, int id, UpdateLocationCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteLocation(ISender sender, int id)
    {
        await sender.Send(new DeleteLocationCommand(id));

        return TypedResults.NoContent();
    }
}
