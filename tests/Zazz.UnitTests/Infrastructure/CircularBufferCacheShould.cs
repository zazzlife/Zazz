using NUnit.Framework;
using Zazz.Infrastructure;

namespace Zazz.UnitTests.Infrastructure
{
    [TestFixture]
    public class CircularBufferCacheShould
    {
        private CircularBufferCache<string, int> _sut;
        private string _key;
        private int _val;
        private const int MAXIMUM_SIZE = 50;

        [SetUp]
        public void Init()
        {
            _sut = new CircularBufferCache<string, int>(MAXIMUM_SIZE);

            _key = "soroush";
            _val = 1;
        }

        [Test]
        public void ShouldAddToItemsOnAdd()
        {
            //Arrange
            //Act
            _sut.Add(_key, _val);

            //Assert
            Assert.IsTrue(_sut.Items.ContainsKey(_key));
            Assert.AreEqual(_val, _sut.Items[_key]);
        }
    }
}