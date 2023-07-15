using GeneratorTests.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GeneratorTests
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Person))]
    internal partial class JsonContext : JsonSerializerContext
    {

    }
    public class JsonTest
    { 
        string expected = $$"""
            {
              "FirstName": "Jim",
              "LastName": "Wooley",
              "Children": []
            }
            """;
        [Fact]
        public void JsonTraditional()
        {
            var person = new Person { FirstName = "Jim", LastName = "Wooley" };
            var json = JsonSerializer.Serialize(person, new JsonSerializerOptions { WriteIndented = true});
            Assert.Equal(expected, json);
        }

        [Fact]
        public void JsonGenerated()
        {
            var person = new Person { FirstName = "Jim", LastName = "Wooley" };
            var json = JsonSerializer.Serialize(person, JsonContext.Default.Person);
            Assert.Equal(expected, json);
        }

    }
}
