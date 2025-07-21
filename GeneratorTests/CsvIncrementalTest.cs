using Generators;
using GeneratorTests.Helpers;

namespace GeneratorTests
{
    // Uses the Verify.SourceGenerators package to test the generator
    // See https://andrewlock.net/creating-a-source-generator-part-2-testing-an-incremental-generator-with-snapshot-testing/
    // for more information
    public class CsvIncrementalTest
    {
        private VerifySettings Settings()
        {
            var settings = new VerifySettings();
            settings.ScrubExpectedChanges();
            settings.UseDirectory("Snapshots");
            settings.UseTypeName(nameof(CsvIncrementalTest));
            settings.DisableRequireUniquePrefix();

            return settings;
        }

        [Fact]
        public async Task TestCsvGeneratorWithoutNamespaceAsync()
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
            var (diagnostics, output) = TestHelpers.GetGeneratedOutput([new CsvIncrementalGenerator()], new(input));
            await Verifier.Verify(output, Settings());
        }


        [Fact]
        public async Task TestCsvGeneratorAsync()
        {
            var input = """
                using CsvIncrementalSerializer;

                namespace TestNamespace;

                [CsvIncrementalSerializable]
                public class Person
                {
                    public string Name { get; set; }
                    public int Age { get; set; }
                }
                """;
            var (diagnostics, output) = TestHelpers.GetGeneratedOutput([new CsvIncrementalGenerator()], new(input));
            await Verifier.Verify(output, Settings());
        }

        [Fact]
        public async Task TestCsvRecordAsync()
        {
            var input = """
                namespace TestNamespace;

                [CsvIncrementalSerializer.CsvIncrementalSerializable]
                public partial record Person (string Name, int Age);
                {
                }
                """;
            var (diagnostics, output) = TestHelpers.GetGeneratedOutput([new CsvIncrementalGenerator()], new(input));
            await Verifier.Verify(output, Settings());
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

            var (diagnostics, output) = TestHelpers.GetGeneratedOutput([new CsvIncrementalGenerator()], new(source));
            await Verifier.Verify(output, Settings());
        }
    }
}

