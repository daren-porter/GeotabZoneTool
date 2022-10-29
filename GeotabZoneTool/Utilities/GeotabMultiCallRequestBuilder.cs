using Geotab.Checkmate.ObjectModel.Captcha;
using Geotab.Checkmate.ObjectModel.Demonstration;
using Geotab.Checkmate.ObjectModel.Files;
using Geotab.Checkmate.ObjectModel.Geographical;
using Geotab.Checkmate.ObjectModel.Registration;

namespace GeotabZoneTool.Utilities;

public class GeotabMultiCallRequestBuilder
{
	// ReSharper disable InconsistentNaming
	// ReSharper disable ParameterHidesMember
	
	private GeotabApiMethod? _method;
	private Type? _entityType;
	private Type? _returnType;
	
	private Credentials? credentials;
	private Entity? entity;
	private Search? search;
	private PropertySelector? propertySelector;
	private CompanyDetails? companyDetails;
	private CaptchaAnswer? captchaAnswer;
	private DemoConfiguration? demoConfiguration;
	private MediaFile? mediaFile;
	private IEnumerable<Coordinate> coordinates;
	private IEnumerable<string> addresses;
	private IEnumerable<Waypoint> waypoints;
	private DeviceSearch? deviceSearch;
	private DateTimeOffset? fromDate;
	private DateTimeOffset? toDate;
	private int? resultsLimit;
	private string? password;
	private string? userName;
	private string? database;
	private string? id;
	private string? timeZoneId;
	private string? fromVersion;
	private bool hosAddresses;
	private bool movingAddresses;

	public GeotabMultiCallRequestBuilder Get<TEntity>(
		Search? entitySearch = null, 
		int? maxResults = null, 
		PropertySelector? propertySelector = null,
		Credentials? credentials = null) where TEntity : Entity?
	{
		_method = GeotabApiMethod.Get;
		_entityType = typeof(TEntity);
		_returnType = typeof(IList<TEntity>);
		
		resultsLimit = maxResults;
		search = entitySearch;
		this.propertySelector = propertySelector;
		this.credentials = credentials;
		
		return this;
	}

	public GeotabMultiCallRequestBuilder Add<TEntity>(TEntity entity, Credentials? credentials = null) 
		where TEntity : Entity
	{
		AddRemoveSet(GeotabApiMethod.Add, entity, credentials);
		return this;
	}
	
	public GeotabMultiCallRequestBuilder Remove<TEntity>(TEntity entity, Credentials? credentials = null) 
		where TEntity : Entity
	{
		AddRemoveSet(GeotabApiMethod.Remove, entity, credentials);
		return this;
	}
	
	public GeotabMultiCallRequestBuilder Set<TEntity>(TEntity entity, Credentials? credentials = null) 
		where TEntity : Entity
	{
		AddRemoveSet(GeotabApiMethod.Set, entity, credentials);
		return this;
	}

	public GeotabMultiCallRequestBuilder GetCoordinates(IEnumerable<string> addresses, Credentials? credentials = null)
	{
		_method = GeotabApiMethod.GetCoordinates;
		_returnType = typeof(IList<Coordinate>);
		_entityType = typeof(Coordinate);
		this.addresses = addresses;
		this.credentials = credentials;

		return this;
	}

	public GeotabMultiCallRequestBuilder ReturnType<T>()
	{
		_returnType = typeof(T);
		return this;
	}

	public object?[] Build()
	{
		switch (_method)
		{
			case GeotabApiMethod.GetCoordinates:
				return new[] { _method?.ToString(), GetParametersForApiMethod(), _returnType };
			default:
				return new[] { _method?.ToString(), _entityType, GetParametersForApiMethod(), _returnType };
		}
	}

	private void AddRemoveSet<TEntity>(GeotabApiMethod method, TEntity entity, Credentials? credentials = null) 
		where TEntity : Entity
	{
		_method = method;
		_entityType = typeof(TEntity);
		this.entity = entity;
		this.credentials = credentials;
	}

	private object? GetParametersForApiMethod()
	{
		return _method switch
		{
			GeotabApiMethod.Authenticate => new { password, userName, database },
			GeotabApiMethod.CreateDatabase => new { companyDetails, database, password, userName, captchaAnswer, demoConfiguration },
			GeotabApiMethod.DatabaseExists => new { database },
			GeotabApiMethod.GenerateCaptcha => new { id },
			GeotabApiMethod.Get => new { credentials, propertySelector, resultsLimit, search },
			GeotabApiMethod.GetAddresses => new { coordinates, credentials, hosAddresses, movingAddresses },
			GeotabApiMethod.GetCoordinates => new { addresses, credentials },
			GeotabApiMethod.GetDaylightSavingRules => new { credentials, timeZoneId },
			GeotabApiMethod.GetFeed => new { credentials, fromVersion, resultsLimit, search },
			GeotabApiMethod.GetRoadMaxSpeeds => new { credentials, deviceSearch, fromDate, toDate },
			GeotabApiMethod.Add 
				or GeotabApiMethod.Remove 
				or GeotabApiMethod.Set => new { credentials, entity },
			GeotabApiMethod.DownloadMediaFile 
				or GeotabApiMethod.UploadMediaFile => new { credentials, mediaFile },
			GeotabApiMethod.GetCountOf 
				or GeotabApiMethod.GetSystemTimeUtc => new { credentials },
			GeotabApiMethod.GetDirections 
				or GeotabApiMethod.OptimizeWaypoints => new { credentials, waypoints },
			GeotabApiMethod.GetTimeZones 
				or GeotabApiMethod.GetVersion 
				or GeotabApiMethod.GetVersionInformation => null,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private enum GeotabApiMethod
	{
		Add, 
		Authenticate, 
		CreateDatabase, 
		DatabaseExists, 
		DownloadMediaFile, 
		GenerateCaptcha,
		Get, 
		GetAddresses,
		GetCoordinates,
		GetCountOf,
		GetDaylightSavingRules,
		GetDirections,
		GetFeed,
		GetRoadMaxSpeeds,
		GetSystemTimeUtc,
		GetTimeZones,
		GetVersion,
		GetVersionInformation,
		OptimizeWaypoints,
		Remove,
		Set,
		UploadMediaFile
	}
}