using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a team consisting of users. A team can own an application.
/// </summary>
public sealed class DiscordTeam : SnowflakeObject, IEquatable<DiscordTeam>
{
	/// <summary>
	/// Gets the team's name.
	/// </summary>
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the team's icon hash.
	/// </summary>
	public string IconHash { get; internal set; }

	/// <summary>
	/// Gets the team's icon.
	/// </summary>
	public string Icon
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.TEAM_ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=1024" : null!;

	/// <summary>
	/// Gets the owner of the team.
	/// </summary>
	public DiscordUser Owner { get; internal set; }

	/// <summary>
	/// Gets the members of this team.
	/// </summary>
	public IReadOnlyList<DiscordTeamMember> Members { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordTeam"/> class.
	/// </summary>
	/// <param name="tt">The tt.</param>
	internal DiscordTeam(TransportTeam tt)
	{
		this.Id = tt.Id;
		this.Name = tt.Name;
		this.IconHash = tt.IconHash;
	}

	/// <summary>
	/// Compares this team to another object and returns whether they are equal.
	/// </summary>
	/// <param name="obj">Object to compare this team to.</param>
	/// <returns>Whether this team is equal to the given object.</returns>
	public override bool Equals(object obj)
		=> obj is DiscordTeam other && this == other;

	/// <summary>
	/// Compares this team to another team and returns whether they are equal.
	/// </summary>
	/// <param name="other">Team to compare to.</param>
	/// <returns>Whether the teams are equal.</returns>
	public bool Equals(DiscordTeam other)
		=> this == other;

	/// <summary>
	/// Gets the hash code of this team.
	/// </summary>
	/// <returns>Hash code of this team.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Converts this team to its string representation.
	/// </summary>
	/// <returns>The string representation of this team.</returns>
	public override string ToString()
		=> $"Team: {this.Name} ({this.Id})";

	public static bool operator ==(DiscordTeam left, DiscordTeam right)
		=> left?.Id == right?.Id;

	public static bool operator !=(DiscordTeam left, DiscordTeam right)
		=> left?.Id != right?.Id;
}

/// <summary>
/// Represents a member of <see cref="DiscordTeam"/>.
/// </summary>
public sealed class DiscordTeamMember : IEquatable<DiscordTeamMember>
{
	/// <summary>
	/// Gets the member's membership status.
	/// </summary>
	public DiscordTeamMembershipStatus MembershipStatus { get; internal set; }

	/// <summary>
	/// Gets the member's permissions within the team.
	/// </summary>
	[DiscordDeprecated]
	public IReadOnlyCollection<string> Permissions { get; internal set; }

	/// <summary>
	/// Gets the member's role within the team.
	/// <para>Can be <c>owner</c>, <c>admin</c>, <c>developer</c> or <c>read-only</c>.</para>
	/// <para>As per official spec, owner won't be transmitted via api, so we fake patch it. Thanks discord..</para>
	/// <para>For those interested, here's the pull request with the owner removal: <see href="https://github.com/discord/discord-api-docs/pull/6384">#6384</see>.</para>
	/// <para>For those with access to ddevs internal: <see href="https://discord.com/channels/613425648685547541/801247546151403531/1144720730613895219">Message in #api at Discord Developers</see>.</para>
	/// </summary>
	public string Role { get; internal set; }

	/// <summary>
	/// Gets the id of the team this member belongs to.
	/// </summary>
	public ulong? TeamId { get; internal set; }

	/// <summary>
	/// Gets the name of the team this member belongs to.
	/// </summary>
	public string TeamName { get; internal set; }

	/// <summary>
	/// Gets the user who is the team member.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordTeamMember"/> class.
	/// </summary>
	/// <param name="ttm">The ttm.</param>
	internal DiscordTeamMember(TransportTeamMember ttm)
	{
		this.MembershipStatus = (DiscordTeamMembershipStatus)ttm.MembershipState;
		this.Permissions = new ReadOnlySet<string>(new HashSet<string>(ttm.Permissions));
		this.Role = ttm.Role;
	}

	/// <summary>
	/// Compares this team member to another object and returns whether they are equal.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether this team is equal to given object.</returns>
	public override bool Equals(object obj)
		=> obj is DiscordTeamMember other && this == other;

	/// <summary>
	/// Compares this team member to another team member and returns whether they are equal.
	/// </summary>
	/// <param name="other">Team member to compare to.</param>
	/// <returns>Whether this team member is equal to the given one.</returns>
	public bool Equals(DiscordTeamMember other)
		=> this == other;

	/// <summary>
	/// Gets a hash code of this team member.
	/// </summary>
	/// <returns>Hash code of this team member.</returns>
	public override int GetHashCode() => HashCode.Combine(this.User, this.TeamId);

	/// <summary>
	/// Converts this team member to their string representation.
	/// </summary>
	/// <returns>String representation of this team member.</returns>
	public override string ToString()
		=> $"Team member: {(this.User.IsMigrated ? this.User.UsernameWithGlobalName : this.User.UsernameWithDiscriminator)} ({this.User.Id}), part of team {this.TeamName} ({this.TeamId})";

	public static bool operator ==(DiscordTeamMember left, DiscordTeamMember right)
		=> left?.TeamId == right?.TeamId && left?.User?.Id == right?.User?.Id;

	public static bool operator !=(DiscordTeamMember left, DiscordTeamMember right)
		=> left?.TeamId != right?.TeamId || left?.User?.Id != right?.User?.Id;
}

/// <summary>
/// Signifies the status of user's team membership.
/// </summary>
public enum DiscordTeamMembershipStatus
{
	/// <summary>
	/// Indicates that this user is invited to the team, and is pending membership.
	/// </summary>
	Invited = 1,

	/// <summary>
	/// Indicates that this user is a member of the team.
	/// </summary>
	Accepted = 2
}
