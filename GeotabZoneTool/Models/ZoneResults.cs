namespace GeotabZoneTool.Models;

public class ZoneResults
{
	public List<ZoneOverlapInfo>? ZonesWithOverlaps { get; set; }
	public List<ZoneOverlapInfo>? InvalidZones { get; set; }
}