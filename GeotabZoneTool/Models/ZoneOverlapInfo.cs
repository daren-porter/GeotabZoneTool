namespace GeotabZoneTool.Models;

public record ZoneOverlapInfo(
	string? Name,
	string? Id,
	IEnumerable<ISimpleCoordinate>? Points = null,
	IEnumerable<ZoneOverlapInfo>? OverlappedBy = null);