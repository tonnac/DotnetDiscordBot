using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// The bool converter.
/// </summary>
public class BoolConverter : IArgumentConverter<bool>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<bool>> IArgumentConverter<bool>.ConvertAsync(string value, CommandContext ctx) =>
		bool.TryParse(value, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<bool>.None);
}

/// <summary>
/// The int8 converter.
/// </summary>
public class Int8Converter : IArgumentConverter<sbyte>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<sbyte>> IArgumentConverter<sbyte>.ConvertAsync(string value, CommandContext ctx) =>
		sbyte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<sbyte>.None);
}

/// <summary>
/// The uint8 converter.
/// </summary>
public class Uint8Converter : IArgumentConverter<byte>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<byte>> IArgumentConverter<byte>.ConvertAsync(string value, CommandContext ctx) =>
		byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<byte>.None);
}

/// <summary>
/// The int16 converter.
/// </summary>
public class Int16Converter : IArgumentConverter<short>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<short>> IArgumentConverter<short>.ConvertAsync(string value, CommandContext ctx) =>
		short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<short>.None);
}

/// <summary>
/// The uint16 converter.
/// </summary>
public class Uint16Converter : IArgumentConverter<ushort>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<ushort>> IArgumentConverter<ushort>.ConvertAsync(string value, CommandContext ctx) =>
		ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<ushort>.None);
}

/// <summary>
/// The int32 converter.
/// </summary>
public class Int32Converter : IArgumentConverter<int>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<int>> IArgumentConverter<int>.ConvertAsync(string value, CommandContext ctx) =>
		int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<int>.None);
}

/// <summary>
/// The uint32 converter.
/// </summary>
public class Uint32Converter : IArgumentConverter<uint>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<uint>> IArgumentConverter<uint>.ConvertAsync(string value, CommandContext ctx) =>
		uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<uint>.None);
}

/// <summary>
/// The int64 converter.
/// </summary>
public class Int64Converter : IArgumentConverter<long>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<long>> IArgumentConverter<long>.ConvertAsync(string value, CommandContext ctx) =>
		long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<long>.None);
}

/// <summary>
/// The uint64 converter.
/// </summary>
public class Uint64Converter : IArgumentConverter<ulong>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<ulong>> IArgumentConverter<ulong>.ConvertAsync(string value, CommandContext ctx) =>
		ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<ulong>.None);
}

/// <summary>
/// The float32 converter.
/// </summary>
public class Float32Converter : IArgumentConverter<float>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<float>> IArgumentConverter<float>.ConvertAsync(string value, CommandContext ctx) =>
		float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<float>.None);
}

/// <summary>
/// The float64 converter.
/// </summary>
public class Float64Converter : IArgumentConverter<double>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<double>> IArgumentConverter<double>.ConvertAsync(string value, CommandContext ctx) =>
		double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<double>.None);
}

/// <summary>
/// The float128 converter.
/// </summary>
public class Float128Converter : IArgumentConverter<decimal>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<decimal>> IArgumentConverter<decimal>.ConvertAsync(string value, CommandContext ctx) =>
		decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<decimal>.None);
}
