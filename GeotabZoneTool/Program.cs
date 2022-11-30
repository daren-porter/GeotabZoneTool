// Update the below constants with the appropriate values for your Geotab database.
// Either a session Id OR username and password are needed, not both
const string username = "";
const string? password = "";
const string? sessionId = null;
const string database = "";
const string? server = "";

var geotabService = new GeotabService(username, password, sessionId, database, server);
var stopwatch = Stopwatch.StartNew();

ConsoleHelper.ToolStartHeader();
ConsoleHelper.WriteLine("Retrieving all zones from Geotab.  This could take a bit.");

var zones = await geotabService.GetAsync<Zone>();
if (zones?.Any() != true)
{
	ConsoleHelper.ReadKeyWithText("No zones were returned from Geotab.");
	return;
}

ConsoleHelper.WriteLine($"{zones.Count} zone(s) retrieved from database.");

// At time of writing, Geotab will allow the creation of zones with impossible coordinates,
// which will cause the API to throw an error if we search.  We will determine which are invalid and
// report to the user that we can't operate on them.
var zoneResults = new ZoneResults { ZonesWithOverlaps = new List<ZoneOverlapInfo>() };
var zonesAreValid = zones.ToLookup(z => geotabService.CoordinatesAreValid(z.Points));
var validZones = zonesAreValid[true].ToList();
var invalidZones = zonesAreValid[false].ToList();

if (invalidZones.Any())
{
	zoneResults.InvalidZones = new List<ZoneOverlapInfo>();
	zoneResults.InvalidZones.AddRange(
		invalidZones.Select(iz => new ZoneOverlapInfo(iz.Name, iz.Id?.ToString(), iz.ExternalReference, iz.Points)));

	ConsoleHelper.MarkupLineInterpolated(
		$"[bold yellow]{invalidZones.Count} zone(s) found with invalid coordinates, which will be skipped. See results for more info.[/]");
}

// Results will be in the order that they were requested
var index = 0;
await foreach (var zoneOverlapResult in geotabService.FindOverlappingZonesAsync(validZones))
{
	try
	{
		var sourceZone = validZones[index];

		// Every request will return at least one zone, the zone that was used to
		// define the search area.  Eliminate that from our results, since we
		// only care about other zones that overlap.
		var overlappingZones = zoneOverlapResult?.Where(o => o.Id != sourceZone.Id);

		if (overlappingZones?.Any() != true)
			continue;

		// Null forgiving reason: The MyGeotab API will always return (and specifically only returns)
		// the ID for nested entities
		zoneResults.ZonesWithOverlaps.Add(new ZoneOverlapInfo(
			sourceZone.Name,
			sourceZone.Id!.ToString(),
			sourceZone.ExternalReference,
			OverlappedBy: overlappingZones.Select(o => new ZoneOverlapInfo(o.Name, o.Id!.ToString(), o.ExternalReference))));
	}
	finally
	{
		if (index == 0) ConsoleHelper.WriteLine();
		Console.Write($"\r{index + 1} zones processed");
		index++;
	}
}

stopwatch.Stop();

if (!zoneResults.ZonesWithOverlaps.Any())
{
	ConsoleHelper.ReadKeyWithText("\nNo overlapping zones found!");
	return;
}

// Output results in JSON to a file
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ZoneResults.json");
await JsonHelper.WriteToJsonFileAsync(zoneResults, outputPath);

// Report final results to the user, then read key to exit
ConsoleHelper.ReportResults(zoneResults, invalidZones.Count, outputPath, stopwatch);
ConsoleHelper.ReadKeyWithText();