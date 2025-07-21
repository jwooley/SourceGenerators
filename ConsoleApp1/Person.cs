using CsvIncrementalSerializer;

namespace ConsoleApp1
{
    [CsvIncrementalSerializable]
    public partial class Person
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }

    [CsvIncrementalSerializable]
    public partial record TestRecord(string FirstName, string LastName)
    {
    }
}