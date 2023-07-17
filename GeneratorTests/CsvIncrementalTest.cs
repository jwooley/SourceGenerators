using Generators;
using GeneratorTests.Helpers;

namespace GeneratorTests
{
    // Uses the Verify.SourceGenerators package to test the generator
    // See https://andrewlock.net/creating-a-source-generator-part-2-testing-an-incremental-generator-with-snapshot-testing/
    // for more information
    [UsesVerify]
    public class CsvIncrementalTest
    {

        [Fact]
        public async Task TestCsvGeneratorAsync()
        { 
            var input = """
                using CsvIncrementalSerializer;

                [CsvIncrementalSerializable]
                public class Person
                {
                    public string Name { get; set; }
                    public int Age { get; set; }
                }
                """;
            await TestHelper.Verify(input, new CsvIncrementalGenerator());
        }

        [Fact]
        public async Task CsvGenerator_WhenMultipleClasses_GeneratesInSeparateRegions()
        {
            // The source code to test
            var source = """
                using CsvIncrementalSerializer;

                [CsvIncrementalSerializable]
                public class Person
                {
                    public string Name { get; set; }
                    public int Age { get; set; }
                }

                public class NotSerialized
                {
                    public string A {get; set; }
                }

                [CsvIncrementalSerializable]
                public class Works
                {
                    public string Col1 {get; set;}
                }
                """;

            await TestHelper.Verify(source, new CsvIncrementalGenerator());
        }
    }
}

