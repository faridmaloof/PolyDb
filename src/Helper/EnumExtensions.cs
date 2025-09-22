using System.ComponentModel;
using System.Reflection;

namespace PolyDb.Helper;

/// <summary>
/// Extension methods for working with enumerations (<see cref="Enum"/>).
/// </summary>
public static class EnumExtensions {
    /// <summary>
    /// Retrieves the description associated with an enumeration value.
    /// </summary>
    /// <param name="value">
    /// The enumeration value for which to retrieve the description.
    /// </param>
    /// <returns>
    /// The string defined in the <see cref="DescriptionAttribute"/> if it exists;
    /// otherwise, the name of the enumeration value.
    /// </returns>
    /// <remarks>
    /// This method uses <see cref="DescriptionAttribute"/> to provide more user-friendly
    /// text for enumeration values. If the attribute is not defined for the specified value,
    /// <c>value.ToString()</c> is returned instead.
    /// </remarks>
    /// <example>
    /// Example usage:
    /// <code>
    /// public enum DatabaseType {
    ///     [Description("Microsoft SQL Server")]
    ///     SqlServer,
    ///     [Description("PostgreSQL")]
    ///     Postgres
    /// }
    ///
    /// // Usage in code:
    /// string desc = DatabaseType.SqlServer.GetDescription();
    /// // Result: "Microsoft SQL Server"
    /// </code>
    /// </example>
    public static string GetDescription(this Enum value) {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        DescriptionAttribute? attribute =
            (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        return attribute?.Description ?? value.ToString();
    }
}