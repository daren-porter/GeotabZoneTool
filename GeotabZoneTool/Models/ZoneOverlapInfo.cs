namespace GeotabZoneTool.Models;

public record ZoneOverlapInfo(
	string? Name,
	string? Id,
	string? ExternalId,
	IEnumerable<ISimpleCoordinate>? Points = null,
	IEnumerable<ZoneOverlapInfo>? OverlappedBy = null);