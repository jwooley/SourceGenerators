using CsvIncrementalSerializer;

namespace ConsoleApp1
{
    [CsvIncrementalSerializable]
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
