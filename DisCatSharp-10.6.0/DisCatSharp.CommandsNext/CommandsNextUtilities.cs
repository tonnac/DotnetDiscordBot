using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.CommandsNext.Converters;
using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.CommandsNext;

/// <summary>
/// Various CommandsNext-related utilities.
/// </summary>
public static class CommandsNextUtilities
{
	/// <summary>
	/// Checks whether the message has a specified string prefix.
	/// </summary>
	/// <param name="msg">Message to check.</param>
	/// <param name="str">String to check for.</param>
	/// <param name="comparisonType">Method of string comparison for the purposes of finding prefixes.</param>
	/// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
	public static int GetStringPrefixLength(this DiscordMessage msg, string str, StringComparison comparisonType = StringComparison.Ordinal)
	{
		var content = msg.Content;
		return str.Length >= content.Length
			? -1
			: !content.StartsWith(str, comparisonType)
				? -1
				: str.Length;
	}

	/// <summary>
	/// Checks whether the message contains a specified mention prefix.
	/// </summary>
	/// <param name="msg">Message to check.</param>
	/// <param name="user">User to check for.</param>
	/// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
	public static int GetMentionPrefixLength(this DiscordMessage msg, DiscordUser user)
	{
		var content = msg.Content;
		if (!content.StartsWith("<@", StringComparison.Ordinal))
			return -1;

		var cni = content.IndexOf('>');
		if (cni == -1 || content.Length <= cni + 1)
			return -1;

		var cnp = content[..(cni + 1)];
		var m = DiscordRegEx.UserRegex().Match(cnp);
		if (!m.Success)
			return -1;

		var userId = ulong.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
		return user.Id != userId ? -1 : m.Value.Length;
	}

	//internal static string ExtractNextArgument(string str, out string remainder)
	/// <summary>
	/// Extracts the next argument.
	/// </summary>
	/// <param name="str">The string.</param>
	/// <param name="startPos">The start position.</param>
	internal static string ExtractNextArgument(this string str, ref int startPos)
	{
		if (string.IsNullOrWhiteSpace(str))
			return null;

		var inBacktick = false;
		var inTripleBacktick = false;
		var inQuote = false;
		var inEscape = false;
		var removeIndices = new List<int>(str.Length - startPos);

		var i = startPos;
		for (; i < str.Length; i++)
			if (!char.IsWhiteSpace(str[i]))
				break;

		startPos = i;

		var endPosition = -1;
		var startPosition = startPos;
		for (i = startPosition; i < str.Length; i++)
		{
			if (char.IsWhiteSpace(str[i]) && !inQuote && !inTripleBacktick && !inBacktick && !inEscape)
				endPosition = i;

			if (str[i] == '\\' && str.Length > i + 1)
			{
				if (!inEscape && !inBacktick && !inTripleBacktick)
				{
					inEscape = true;
					if (str.IndexOf("\\`", i) == i || str.IndexOf("\\\"", i) == i || str.IndexOf("\\\\", i) == i || (str.Length >= i && char.IsWhiteSpace(str[i + 1])))
						removeIndices.Add(i - startPosition);
					i++;
				}
				else if ((inBacktick || inTripleBacktick) && str.IndexOf("\\`", i) == i)
				{
					inEscape = true;
					removeIndices.Add(i - startPosition);
					i++;
				}
			}

			if (str[i] == '`' && !inEscape)
			{
				var tripleBacktick = str.IndexOf("```", i) == i;
				if (inTripleBacktick && tripleBacktick)
				{
					inTripleBacktick = false;
					i += 2;
				}
				else if (!inBacktick && tripleBacktick)
				{
					inTripleBacktick = true;
					i += 2;
				}

				if (inBacktick && !tripleBacktick)
					inBacktick = false;
				else if (!inTripleBacktick && tripleBacktick)
					inBacktick = true;
			}

			if (str[i] == '"' && !inEscape && !inBacktick && !inTripleBacktick)
			{
				removeIndices.Add(i - startPosition);

				inQuote = !inQuote;
			}

			if (inEscape)
				inEscape = false;

			if (endPosition != -1)
			{
				startPos = endPosition;
				return startPosition != endPosition ? str[startPosition..endPosition].CleanupString(removeIndices) : null;
			}
		}

		startPos = str.Length;
		return startPos != startPosition ? str[startPosition..].CleanupString(removeIndices) : null;
	}

	/// <summary>
	/// Cleanups the string.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="indices">The indices.</param>
	internal static string CleanupString(this string s, IList<int> indices)
	{
		if (!indices.Any())
			return s;

		var li = indices.Last();
		var ll = 1;
		for (var x = indices.Count - 2; x >= 0; x--)
		{
			if (li - indices[x] == ll)
			{
				ll++;
				continue;
			}

			s = s.Remove(li - ll + 1, ll);
			li = indices[x];
			ll = 1;
		}

		return s.Remove(li - ll + 1, ll);
	}

	/// <summary>
	/// Binds the arguments.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="ignoreSurplus">If true, ignore further text in string.</param>
	internal static async Task<ArgumentBindingResult> BindArguments(CommandContext ctx, bool ignoreSurplus)
	{
		var command = ctx.Command;
		var overload = ctx.Overload;

		var args = new object[overload.Arguments.Count + 2];
		args[1] = ctx;
		var rawArgumentList = new List<string>(overload.Arguments.Count);

		var argString = ctx.RawArgumentString;
		var foundAt = 0;
		foreach (var arg in overload.Arguments)
		{
			string argValue;
			if (arg.IsCatchAll)
			{
				if (arg.IsArray)
				{
					while (true)
					{
						argValue = ExtractNextArgument(argString, ref foundAt);
						if (argValue == null)
							break;

						rawArgumentList.Add(argValue);
					}

					break;
				}
				else
				{
					if (argString == null)
						break;

					argValue = argString[foundAt..].Trim();
					argValue = argValue == "" ? null : argValue;
					foundAt = argString.Length;

					rawArgumentList.Add(argValue);
					break;
				}
			}
			else
			{
				argValue = ExtractNextArgument(argString, ref foundAt);
				rawArgumentList.Add(argValue);
			}

			if (argValue == null && arg is { IsOptional: false, IsCatchAll: false })
				return new(new ArgumentException("Not enough arguments supplied to the command."));
			else if (argValue == null)
				rawArgumentList.Add(null);
		}

		if (!ignoreSurplus && foundAt < argString.Length)
			return new(new ArgumentException("Too many arguments were supplied to this command."));

		for (var i = 0; i < overload.Arguments.Count; i++)
		{
			var arg = overload.Arguments[i];
			if (arg is { IsCatchAll: true, IsArray: true })
			{
				var array = Array.CreateInstance(arg.Type, rawArgumentList.Count - i);
				var start = i;
				while (i < rawArgumentList.Count)
				{
					try
					{
						array.SetValue(await ctx.CommandsNext.ConvertArgument(rawArgumentList[i], ctx, arg.Type).ConfigureAwait(false), i - start);
					}
					catch (Exception ex)
					{
						return new(ex);
					}

					i++;
				}

				args[start + 2] = array;
				break;
			}
			else
				try
				{
					args[i + 2] = rawArgumentList[i] != null ? await ctx.CommandsNext.ConvertArgument(rawArgumentList[i], ctx, arg.Type).ConfigureAwait(false) : arg.DefaultValue;
				}
				catch (Exception ex)
				{
					return new(ex);
				}
		}

		return new(args, rawArgumentList);
	}

	/// <summary>
	/// Whether this module is a candidate type.
	/// </summary>
	/// <param name="type">The type.</param>
	internal static bool IsModuleCandidateType(this Type type)
		=> type.GetTypeInfo().IsModuleCandidateType();

	/// <summary>
	/// Whether this module is a candidate type.
	/// </summary>
	/// <param name="ti">The type info.</param>
	internal static bool IsModuleCandidateType(this TypeInfo ti)
	{
		// check if compiler-generated
		if (ti.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
			return false;

		// check if derives from the required base class
		var tmodule = typeof(BaseCommandModule);
		var timodule = tmodule.GetTypeInfo();
		if (!timodule.IsAssignableFrom(ti))
			return false;

		// check if anonymous
		if (ti.IsGenericType && ti.Name.Contains("AnonymousType") && (ti.Name.StartsWith("<>", StringComparison.Ordinal) || ti.Name.StartsWith("VB$", StringComparison.Ordinal)) && (ti.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
			return false;

		// check if abstract, static, or not a class
		if (!ti.IsClass || ti.IsAbstract)
			return false;

		// check if delegate type
		var tdelegate = typeof(Delegate).GetTypeInfo();
		if (tdelegate.IsAssignableFrom(ti))
			return false;

		// qualifies if any method or type qualifies
		return ti.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _)) || ti.DeclaredNestedTypes.Any(xti => xti.IsModuleCandidateType());
	}

	/// <summary>
	/// Whether this is a command candidate.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="parameters">The parameters.</param>
	internal static bool IsCommandCandidate(this MethodInfo method, out ParameterInfo[] parameters)
	{
		parameters = null;
		// check if exists
		if (method == null)
			return false;

		// check if static, non-public, abstract, a constructor, or a special name
		if (method.IsStatic || method.IsAbstract || method.IsConstructor || method.IsSpecialName)
			return false;

		// check if appropriate return and arguments
		parameters = method.GetParameters();

		return parameters.Length != 0 && parameters.First().ParameterType == typeof(CommandContext) && method.ReturnType == typeof(Task);
	}

	/// <summary>
	/// Creates the instance.
	/// </summary>
	/// <param name="t">The type.</param>
	/// <param name="services">The services provider.</param>
	internal static object CreateInstance(this Type t, IServiceProvider services)
	{
		var ti = t.GetTypeInfo();
		var constructors = ti.DeclaredConstructors
			.Where(xci => xci.IsPublic)
			.ToArray();

		if (constructors.Length != 1)
			throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");

		var constructor = constructors[0];
		var constructorArgs = constructor.GetParameters();
		var args = new object[constructorArgs.Length];

		if (constructorArgs.Length != 0 && services == null)
			throw new InvalidOperationException("Dependency collection needs to be specified for parameterized constructors.");

		// inject via constructor
		if (constructorArgs.Length != 0)
			for (var i = 0; i < args.Length; i++)
				args[i] = services.GetRequiredService(constructorArgs[i].ParameterType);

		var moduleInstance = Activator.CreateInstance(t, args);

		// inject into properties
		var props = t.GetRuntimeProperties().Where(xp => xp.CanWrite && xp.SetMethod != null && !xp.SetMethod.IsStatic && xp.SetMethod.IsPublic);
		foreach (var prop in props)
		{
			if (prop.GetCustomAttribute<DontInjectAttribute>() != null)
				continue;

			var service = services.GetService(prop.PropertyType);
			if (service == null)
				continue;

			prop.SetValue(moduleInstance, service);
		}

		// inject into fields
		var fields = t.GetRuntimeFields().Where(xf => !xf.IsInitOnly && xf is { IsStatic: false, IsPublic: true });
		foreach (var field in fields)
		{
			if (field.GetCustomAttribute<DontInjectAttribute>() != null)
				continue;

			var service = services.GetService(field.FieldType);
			if (service == null)
				continue;

			field.SetValue(moduleInstance, service);
		}

		return moduleInstance;
	}
}
