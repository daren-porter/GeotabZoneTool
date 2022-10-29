using Geotab.Checkmate;

namespace GeotabZoneTool.Services;

public sealed class GeotabApiService
{
	private readonly API _api;

	public GeotabApiService(
		string username, 
		string? password, 
		string? sessionId,
		string database,
		string? server,
		int timeout = 300000,
		HttpMessageHandler? handler = null)
	{
		_api = new API(username, password, sessionId, database, server, timeout, handler);
	}

	public async Task<IList<TEntity>?> GetAsync<TEntity>(Search? search = null) where TEntity : Entity
	{
		await EnsureAuthenticatedAsync();

		return await _api.CallAsync<IList<TEntity>>("Get", typeof(TEntity), new { search });
	}

	public async Task<List<object?>> MultiCallAsync(object[] calls)
	{
		await EnsureAuthenticatedAsync();
		
		return await _api.MultiCallAsync(calls);
	}

	private async Task EnsureAuthenticatedAsync()
	{
		// Object will already have credentials if we have already authenticated
		if (_api.LoginResult?.Credentials is null)
		{
			await _api.AuthenticateAsync();
		}
	}
}