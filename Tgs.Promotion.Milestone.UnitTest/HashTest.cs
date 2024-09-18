using Tgs.Promotion.Milestone.Core;

namespace Tgs.Promotion.Milestone.UnitTest
{
    public class HashTest
    {
        [Theory]
        [InlineData("4uk417f4djjr1")]
        [InlineData("4uk417f4djjr2")]
        [InlineData("4uk417f4djjr3")]
        public void Hash_Test(string code)
        {
            var code1 = ComputeSha256Hash.ComputeSha256HashToLong(code);
            var code2 = ComputeSha256Hash.ComputeSha256HashToLong(code);

            Assert.Equal(code1, code2);
        }
    }
}
