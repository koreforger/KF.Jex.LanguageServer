namespace KF.Jex.LanguageServer.Services;

/// <summary>
/// Provides information about the JEX standard library functions.
/// </summary>
public static class StandardLibraryProvider
{
    private static readonly List<StdLibFunction> _functions = new()
    {
        // JsonPath functions
        new("jp1", "jp1(json, path)", "Returns the first token at the JSONPath. Returns null if not found.", 2, 2),
        new("jpAll", "jpAll(json, path)", "Returns an array of all tokens matching the JSONPath.", 2, 2),
        new("jpFirst", "jpFirst(json, ...paths)", "Returns the first non-null result from multiple JSONPaths.", 2, int.MaxValue),
        new("existsPath", "existsPath(json, path)", "Returns true if the JSONPath exists and is not null.", 2, 2),

        // String functions
        new("trim", "trim(s)", "Removes leading and trailing whitespace.", 1, 1),
        new("lower", "lower(s)", "Converts string to lowercase.", 1, 1),
        new("upper", "upper(s)", "Converts string to uppercase.", 1, 1),
        new("substring", "substring(s, start, length?)", "Extracts a substring.", 2, 3),
        new("replace", "replace(s, old, new)", "Replaces all occurrences of a substring.", 3, 3),
        new("split", "split(s, delimiter)", "Splits a string into an array.", 2, 2),
        new("join", "join(array, delimiter)", "Joins array elements into a string.", 2, 2),
        new("startsWith", "startsWith(s, prefix)", "Returns true if string starts with prefix.", 2, 2),
        new("endsWith", "endsWith(s, suffix)", "Returns true if string ends with suffix.", 2, 2),
        new("contains", "contains(s, substring)", "Returns true if string contains substring.", 2, 2),
        new("indexOf", "indexOf(s, substring)", "Returns the index of substring, or -1 if not found.", 2, 2),
        new("concat", "concat(...values)", "Concatenates all arguments as strings.", 0, int.MaxValue),
        new("length", "length(s)", "Returns the length of a string or array.", 1, 1),
        new("padLeft", "padLeft(s, length, char?)", "Pads string on the left to specified length.", 2, 3),
        new("padRight", "padRight(s, length, char?)", "Pads string on the right to specified length.", 2, 3),
        new("format", "format(template, ...args)", "Formats a string with placeholders {0}, {1}, etc.", 1, int.MaxValue),
        new("regex", "regex(s, pattern)", "Returns true if string matches regex pattern.", 2, 2),
        new("regexReplace", "regexReplace(s, pattern, replacement)", "Replaces regex matches.", 3, 3),
        new("regexMatch", "regexMatch(s, pattern)", "Returns the first regex match or null.", 2, 2),
        new("regexMatches", "regexMatches(s, pattern)", "Returns all regex matches as an array.", 2, 2),

        // Math functions
        new("abs", "abs(n)", "Returns the absolute value.", 1, 1),
        new("round", "round(n, decimals?)", "Rounds to specified decimal places.", 1, 2),
        new("floor", "floor(n)", "Rounds down to nearest integer.", 1, 1),
        new("ceiling", "ceiling(n)", "Rounds up to nearest integer.", 1, 1),
        new("min", "min(...values)", "Returns the minimum value.", 1, int.MaxValue),
        new("max", "max(...values)", "Returns the maximum value.", 1, int.MaxValue),
        new("sum", "sum(array)", "Returns the sum of array elements.", 1, 1),
        new("avg", "avg(array)", "Returns the average of array elements.", 1, 1),

        // Date functions
        new("parseDate", "parseDate(s, format?, timezone?)", "Parses a date string.", 1, 3),
        new("formatDate", "formatDate(date, format)", "Formats a date as string.", 2, 2),
        new("now", "now()", "Returns the current UTC date/time.", 0, 0),
        new("today", "today()", "Returns the current UTC date (no time).", 0, 0),
        new("addDays", "addDays(date, days)", "Adds days to a date.", 2, 2),
        new("addMonths", "addMonths(date, months)", "Adds months to a date.", 2, 2),
        new("addYears", "addYears(date, years)", "Adds years to a date.", 2, 2),
        new("diffDays", "diffDays(date1, date2)", "Returns the difference in days.", 2, 2),

        // Type functions
        new("toString", "toString(value)", "Converts value to string.", 1, 1),
        new("toNumber", "toNumber(value)", "Converts value to number.", 1, 1),
        new("toBool", "toBool(value)", "Converts value to boolean.", 1, 1),
        new("toDate", "toDate(value)", "Converts value to date.", 1, 1),
        new("isNull", "isNull(value)", "Returns true if value is null.", 1, 1),
        new("isEmpty", "isEmpty(value)", "Returns true if value is null, empty string, or empty array/object.", 1, 1),
        new("isString", "isString(value)", "Returns true if value is a string.", 1, 1),
        new("isNumber", "isNumber(value)", "Returns true if value is a number.", 1, 1),
        new("isBool", "isBool(value)", "Returns true if value is a boolean.", 1, 1),
        new("isArray", "isArray(value)", "Returns true if value is an array.", 1, 1),
        new("isObject", "isObject(value)", "Returns true if value is an object.", 1, 1),

        // Array functions
        new("first", "first(array)", "Returns the first element.", 1, 1),
        new("last", "last(array)", "Returns the last element.", 1, 1),
        new("take", "take(array, count)", "Returns the first N elements.", 2, 2),
        new("skip", "skip(array, count)", "Skips the first N elements.", 2, 2),
        new("reverse", "reverse(array)", "Reverses the array.", 1, 1),
        new("sort", "sort(array)", "Sorts the array.", 1, 1),
        new("distinct", "distinct(array)", "Returns unique elements.", 1, 1),
        new("flatten", "flatten(array)", "Flattens nested arrays.", 1, 1),
        new("count", "count(array)", "Returns the number of elements.", 1, 1),
        new("any", "any(array)", "Returns true if array has any elements.", 1, 1),
        new("all", "all(array, predicate)", "Returns true if all elements match predicate.", 2, 2),
        new("where", "where(array, predicate)", "Filters array by predicate.", 2, 2),
        new("select", "select(array, selector)", "Projects each element.", 2, 2),
        new("groupBy", "groupBy(array, keySelector)", "Groups elements by key.", 2, 2),
        new("orderBy", "orderBy(array, keySelector)", "Orders elements by key.", 2, 2),
        new("push", "push(array, item)", "Adds item to end of array (modifies $out).", 2, 2),

        // Output functions
        new("newObject", "newObject()", "Creates a new empty JSON object.", 0, 0),
        new("newArray", "newArray()", "Creates a new empty JSON array.", 0, 0),
        new("merge", "merge(target, source)", "Merges source object into target.", 2, 2),
        new("clone", "clone(value)", "Creates a deep copy of a JSON value.", 1, 1),
        new("remove", "remove(obj, path)", "Removes a property from an object.", 2, 2),
        new("expandJson", "expandJson(value)", "Recursively parses embedded JSON strings.", 1, 1),

        // Utility functions
        new("coalesce", "coalesce(...values)", "Returns the first non-null value.", 1, int.MaxValue),
        new("iif", "iif(condition, trueValue, falseValue)", "Returns trueValue if condition is true, else falseValue.", 3, 3),
        new("default", "default(value, defaultValue)", "Returns defaultValue if value is null.", 2, 2),
        new("guid", "guid()", "Generates a new GUID string.", 0, 0),
        new("hash", "hash(value, algorithm?)", "Computes hash of value (default: SHA256).", 1, 2),
    };

    /// <summary>
    /// Gets all standard library functions.
    /// </summary>
    public static IReadOnlyList<StdLibFunction> GetFunctions() => _functions;

    /// <summary>
    /// Gets a function by name (case-insensitive).
    /// </summary>
    public static StdLibFunction? GetFunction(string name)
    {
        return _functions.FirstOrDefault(f => 
            string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}

public record StdLibFunction(string Name, string Signature, string Description, int MinArgs, int MaxArgs);
