// See https://aka.ms/new-console-template for more information
using ConsoleApp1;
using CsvIncrementalSerializer;

Console.WriteLine(new Person { FirstName = "John", LastName = "Doe" }.ToCsv());