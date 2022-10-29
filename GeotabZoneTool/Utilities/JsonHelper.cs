using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeotabZoneTool.Utilities;

public static class JsonHelper
{
	public static async Task WriteToJsonFileAsync(object objToWrite, string path)
	{
		var options = new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		
		var json = JsonSerializer.Serialize(objToWrite, options);
		await File.WriteAllTextAsync(path, json);
	}
}