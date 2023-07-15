using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeneratorTests
{
    public partial class RegexText
    {
        [Theory]
        [InlineData("", false)]
        [InlineData("jimw@slalom.com", true)]
        public void TestEmailValidation(string input, bool shouldPass)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            var match = regex.Match(input);
            Assert.Equal(shouldPass, match.Success);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("jimw@slalom.com", true)]
        public void TestEmailValidationOptimized(string input, bool shouldPass)
        {
            Assert.Equal(shouldPass, EmailRegex().IsMatch(input));
        }

        [GeneratedRegex(@"([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex EmailRegex();


    }
}
