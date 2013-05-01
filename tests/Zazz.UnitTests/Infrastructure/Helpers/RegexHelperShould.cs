using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Infrastructure.Helpers;

namespace Zazz.UnitTests.Infrastructure.Helpers
{
    [TestFixture]
    public class RegexHelperShould
    {
        private RegexHelper _sut;

        [SetUp]
        public void Init()
        {
            _sut = new RegexHelper();
        }

        [Test]
        public void ReturnCorrectWords_OnExtractTags()
        {
            //Arrange
            var tag1 = "#TEXT";
            var tag2 = "#to";
            var tag3 = "#messed-up";
            var tag4 = "#Characters";
            var tag5 = "#Line";
            var tag6 = "#ASDF-Gh";
            var tag7 = "#abc-d";

            var sampleText = String.Format("This {0} is going {1} contain some {2} {3} + new text-with-dash \nalso testing a new {4} {5}. adsf {6}", tag1, tag2, tag3, tag4, tag5, tag6, tag7);

            //Act
            var result = _sut.ExtractTags(sampleText).ToList();

            //Assert
            Assert.AreEqual(7, result.Count);
            CollectionAssert.Contains(result, tag1);
            CollectionAssert.Contains(result, tag2);
            CollectionAssert.Contains(result, tag3);
            CollectionAssert.Contains(result, tag4);
            CollectionAssert.Contains(result, tag5);
            CollectionAssert.Contains(result, tag6);
            CollectionAssert.Contains(result, tag7);
        }
    }
}