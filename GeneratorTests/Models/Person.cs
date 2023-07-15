namespace GeneratorTests.Models;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public List<Person> Children { get; set; } = new List<Person>();
}
