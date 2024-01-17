using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildMemberUpdated"/> event.
/// </summary>
public class GuildMemberUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets a collection containing post-update roles.
	/// </summary>
	public IReadOnlyList<DiscordRole> RolesAfter { get; internal set; }

	/// <summary>
	/// Gets a collection containing pre-update roles.
	/// </summary>
	public IReadOnlyList<DiscordRole> RolesBefore { get; internal set; }

	/// <summary>
	/// Gets the member's new nickname.
	/// </summary>
	public string NicknameAfter { get; internal set; }

	/// <summary>
	/// Gets the member's old nickname.
	/// </summary>
	public string NicknameBefore { get; internal set; }

	/// <summary>
	/// Gets whether the member had passed membership screening before the update.
	/// </summary>
	public bool? PendingBefore { get; internal set; }

	/// <summary>
	/// Gets whether the member had passed membership screening after the update.
	/// </summary>
	public bool? PendingAfter { get; internal set; }

	/// <summary>
	/// Gets whether the member is timed out before the update.
	/// </summary>
	public DateTimeOffset? TimeoutBefore { get; internal set; }

	/// <summary>
	/// Gets whether the member is timed out after the update.
	/// </summary>
	public DateTimeOffset? TimeoutAfter { get; internal set; }

	/// <summary>
	/// Gets whether the member has unusual dm activity before the update.
	/// </summary>
	public DateTimeOffset? UnusualDmActivityBefore { get; internal set; }

	/// <summary>
	/// Gets whether the member has unusual dm activity after the update.
	/// </summary>
	public DateTimeOffset? UnusualDmActivityAfter { get; internal set; }

	/// <summary>
	/// Gets the member that was updated.
	/// </summary>
	public DiscordMember Member { get; internal set; }

	public virtual string GuildAvatarHashBefore { get; internal set; }

	public string GuildAvatarUrlBefore
		=> string.IsNullOrWhiteSpace(this.GuildAvatarHashBefore) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GUILDS}/{this.Guild.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.USERS}/{this.Member.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.AVATARS}/{this.GuildAvatarHashBefore}.{(this.GuildAvatarHashBefore.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	public virtual string GuildAvatarHashAfter { get; internal set; }

	public string GuildAvatarUrlAfter
		=> string.IsNullOrWhiteSpace(this.GuildAvatarHashAfter) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GUILDS}/{this.Guild.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.USERS}/{this.Member.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.AVATARS}/{this.GuildAvatarHashAfter}.{(this.GuildAvatarHashAfter.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	public virtual string AvatarHashBefore { get; internal set; }

	public string AvatarUrlBefore
		=> string.IsNullOrWhiteSpace(this.AvatarHashBefore) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Member.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHashBefore}.{(this.AvatarHashBefore.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	public virtual string AvatarHashAfter { get; internal set; }

	public string AvatarUrlAfter
		=> string.IsNullOrWhiteSpace(this.AvatarHashAfter) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Member.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHashAfter}.{(this.AvatarHashAfter.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildMemberUpdateEventArgs"/> class.
	/// </summary>
	internal GuildMemberUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
