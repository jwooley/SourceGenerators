//HintName: CsvIncrementalSerializer.g.cs
using System.Linq;
namespace CsvIncrementalSerializer;

public static class GeneratedIncrementalSerializer
{
    #region Person
    public static string ToCsv(this Person input) =>
        $"\"{input.Name}\", {input.Age}";

    public static string ToCsvHeader(this Person input) =>
        "Name, Age";

    public static string ToCsv(this System.Collections.Generic.IEnumerable<Person> input)
    {
        var sb = new System.Text.StringBuilder();
        if (input.Any())
        {
            sb.AppendLine(input.First().ToCsvHeader());
            foreach (var item in input)
            {
                sb.AppendLine(item.ToCsv());
            }
        }
        return sb.ToString();
    }

    #endregion

    #region Works
    public static string ToCsv(this Works input) =>
        $"\"{input.Col1}\"";

    public static string ToCsvHeader(this Works input) =>
        "Col1";

    public static string ToCsv(this System.Collections.Generic.IEnumerable<Works> input)
    {
        var sb = new System.Text.StringBuilder();
        if (input.Any())
        {
            sb.AppendLine(input.First().ToCsvHeader());
            foreach (var item in input)
            {
                sb.AppendLine(item.ToCsv());
            }
        }
        return sb.ToString();
    }

    #endregion

}