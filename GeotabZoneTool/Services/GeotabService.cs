using System.Timers;
using Geotab.Checkmate.ObjectModel.Geographical;
using Timer = System.Timers.Timer;

namespace GeotabZoneTool.Services;

public class GeotabService
{
    private readonly GeotabApiService _apiService;

    public GeotabService(
        string username,
        string? password,
        string? sessionId,
        string database,
        string? server,
        int timeout = 300000,
        HttpMessageHandler? handler = null)
    {
        _apiService = new GeotabApiService(username, password, sessionId, database, server, timeout, handler);
    }

    public async Task<IList<TEntity>?> GetAsync<TEntity>(Search? search = null) where TEntity : Entity =>
        await _apiService.GetAsync<TEntity>(search);
    
    public async IAsyncEnumerable<IEnumerable<Zone>?> FindOverlappingZonesAsync(IList<Zone> sourceZones)
    {
        var multiCalls = BuildOverlappingZoneMultiCallRequests(sourceZones);
        var results = Array.Empty<List<object?>>();
        
        // Make batched multi-call requests and report progress to console
        await ConsoleHelper.StartProgressTaskAsync(async ctx =>
        {
            var batchCalls = multiCalls.Chunk(1000).Select((batch, i) => BatchCallAsync(batch, i + 1, ctx));
            results = await Task.WhenAll(batchCalls);
        });

        foreach (var result in results.SelectMany(x => x))
        {
            yield return result as IEnumerable<Zone>;
        }
    }
    
    private static IEnumerable<object> BuildOverlappingZoneMultiCallRequests(IEnumerable<Zone> zones)
    {
        // Create a ZoneSearch for each zone with a SearchArea that matches the dimensions of the zone itself.
        // Null forgiving reason: Geotab requires zones to have at least 3 points
        var zoneSearches = zones.Select(z => new ZoneSearch
        {
            SearchArea = new BoundingBox
            {
                Left = z.Points!.Select(p => p.X).MinBy(x => x),
                Right = z.Points!.Select(p => p.X).MaxBy(x => x),
                Top = z.Points!.Select(p => p.Y).MaxBy(y => y),
                Bottom = z.Points!.Select(p => p.Y).MinBy(y => y)
            }
        });
        
        // Build a multi-call request for each zone search
        var builder = new GeotabMultiCallRequestBuilder();
        return zoneSearches.Select(zoneSearch => builder.Get<Zone>(zoneSearch).Build());
    }
    
    private Task<List<object?>> BatchCallAsync(object[] batch, int batchNum, ProgressContext ctx)
    {
        // Start a running Task that will make a multi-call and update console
        // with its individual progress
        return Task.Run(async () =>
        {
            var task = ctx.AddTask($"Requesting batch #{batchNum}... ");
            task.StartTask();

            void Tick(object? obj, ElapsedEventArgs e) => task.Increment(1);
            var timer = new Timer(500);
            timer.Elapsed += Tick;
            timer.Start();

            var result = await _apiService.MultiCallAsync(batch);

            // Remove progress bar incremental update and complete.
            timer.Stop();
            timer.Elapsed -= Tick;
            timer.Dispose();
            task.Increment(100 - task.Percentage);
            task.Description = $"Batch #{batchNum} [green]Done![/] ";
            task.StopTask();

            return result;
        });
    }
    
    public bool CoordinatesAreValid(IEnumerable<ISimpleCoordinate>? points) => 
        points?.All(p => p.X is >= -180 and <= 180 && p.Y is >= -90 and <= 90) == true;
        
}