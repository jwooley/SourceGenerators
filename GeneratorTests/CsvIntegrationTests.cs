using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvIncrementalSerializer;

namespace GeneratorTests
{
    public class CsvIntegrationTests
    {
        [Fact]
        public void CanSerializeClass()
        {
            var testObject = new TestClass { Name = "John Doe", Age = 30, IsActive = true };
            var csv = testObject.ToCsv();
            Assert.Equal("\"John Doe\",30,True", csv);
        }

        [Fact]
        public void CanSerializeListOfClasses()
        {
            var testObjects = new List<TestClass>
            {
                new TestClass { Name = "John Doe", Age = 30, IsActive = true },
                new TestClass { Name = "Jane Smith", Age = 25, IsActive = false }
            };
            var csv = TestClass.ToCsv(testObjects);
            Assert.Equal("Name,Age,IsActive\r\n\"John Doe\",30,True\r\n\"Jane Smith\",25,False", csv);
        }
    }

    [CsvIncrementalSerializable]
    public partial class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }
}
