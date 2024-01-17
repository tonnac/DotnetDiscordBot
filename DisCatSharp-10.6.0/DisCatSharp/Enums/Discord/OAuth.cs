// ReSharper disable InconsistentNaming

namespace DisCatSharp.Enums;

/// <summary>
/// The oauth scopes.
/// </summary>
public static class OAuth
{
	/// <summary>
	/// The default scopes for bots.
	/// </summary>
	private const string BOT_DEFAULT = "bot applications.commands"; // applications.commands.permissions.update

	/// <summary>
	/// The bot minimal scopes.
	/// </summary>
	private const string BOT_MINIMAL = "bot applications.commands";

	/// <summary>
	/// The bot only scope.
	/// </summary>
	private const string BOT_ONLY = "bot";

	/// <summary>
	/// The basic identify scopes.
	/// </summary>
	private const string IDENTIFY_BASIC = "identify email";

	/// <summary>
	/// The extended identify scopes.
	/// </summary>
	private const string IDENTIFY_EXTENDED = "identify email guilds guilds.members.read connections";

	/// <summary>
	/// The role connection scope.
	/// </summary>
	private const string ROLE_CONNECTIONS_WRITE = "role_connections.write";

	/// <summary>
	/// All scopes for bots and identify.
	/// </summary>
	private const string ALL = BOT_DEFAULT + " " + IDENTIFY_EXTENDED + " " + ROLE_CONNECTIONS_WRITE;

	/// <summary>
	/// Resolves the scopes.
	/// </summary>
	/// <param name="scope">The scope.</param>
	/// <returns>A string representing the scopes.</returns>
	public static string ResolveScopes(OAuthScopes scope) =>
		scope switch
		{
			OAuthScopes.BOT_DEFAULT => BOT_DEFAULT,
			OAuthScopes.BOT_MINIMAL => BOT_MINIMAL,
			OAuthScopes.BOT_ONLY => BOT_ONLY,
			OAuthScopes.IDENTIFY_BASIC => IDENTIFY_BASIC,
			OAuthScopes.IDENTIFY_EXTENDED => IDENTIFY_EXTENDED,
			OAuthScopes.ALL => ALL,
			_ => BOT_DEFAULT
		};
}

/// <summary>
/// The oauth scopes.
/// </summary>
public enum OAuthScopes
{
	/// <summary>
	/// Scopes: bot applications.commands (Excluding applications.commands.permissions.update for now)
	/// </summary>
	BOT_DEFAULT = 0,

	/// <summary>
	/// Scopes: bot applications.commands
	/// </summary>
	BOT_MINIMAL = 1,

	/// <summary>
	/// Scopes: bot
	/// </summary>
	BOT_ONLY = 2,

	/// <summary>
	/// Scopes: identify email
	/// </summary>
	IDENTIFY_BASIC = 3,

	/// <summary>
	/// Scopes: identify email guilds connections
	/// </summary>
	IDENTIFY_EXTENDED = 4,

	/// <summary>
	/// Scopes: bot applications.commands applications.commands.permissions.update identify email guilds connections role_connections.write
	/// </summary>
	ALL = 5,

	/// <summary>
	/// Scopes: role_connections.write
	/// </summary>
	ROLE_CONNECTIONS_WRITE = 6
}
