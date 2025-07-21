// See https://aka.ms/new-console-template for more information
using ConsoleApp1;

Console.WriteLine(new Person { FirstName = "John", LastName = "Doe" }.ToCsv());

var people = new[]
{
    new Person { FirstName = "John", LastName = "Doe" },
    new Person { FirstName = "Jane", LastName = "Smith" }
};
Console.WriteLine(Person.ToCsv(people));

var testRecord = new TestRecord("Alice", "Johnson");
Console.WriteLine(testRecord.ToCsv());
