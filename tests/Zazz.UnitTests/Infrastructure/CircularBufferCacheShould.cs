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
        private const int MAXIMUM_SIZE = 2;

        [SetUp]
        public void Init()
        {
            _sut = new CircularBufferCache<string, int>(MAXIMUM_SIZE);

            _key = "soroush";
            _val = 1;
        }

        [Test]
        public void ShouldAddToItemsAndCounterOnAdd()
        {
            //Arrange
            //Act
            _sut.Add(_key, _val);

            //Assert
            Assert.IsTrue(_sut.Items.ContainsKey(_key));
            Assert.AreEqual(_val, _sut.Items[_key]);
            Assert.IsTrue(_sut.RequestsCounter.ContainsKey(_key));
            Assert.AreEqual(0, _sut.RequestsCounter[_key]);
        }

        [Test]
        public void NotThrowIfItemIsAlreadyAdded_OnAdd_ToBeThreadSafe()
        {
            //Arrange
            _sut.Items.TryAdd(_key, _val);
            _sut.RequestsCounter.TryAdd(_key, 0);

            Assert.IsTrue(_sut.Items.ContainsKey(_key));
            Assert.IsTrue(_sut.RequestsCounter.ContainsKey(_key));

            //Act
            _sut.Add(_key, _val);

            //Assert
            Assert.Pass("No Exception was thrown");
        }

        [Test]
        public void RemoveTheItemWithLowestRequestsCountIfItIsAtMaximumCapacity_OnAdd()
        {
            //Arranges
            var key2 = "key2";
            var val2 = 2;
            var key3 = "key3";
            var val3 = 3;

            _sut.Items.TryAdd(_key, _val);
            _sut.RequestsCounter.TryAdd(_key, 1);
            _sut.Items.TryAdd(key2, val2);
            _sut.RequestsCounter.TryAdd(key2, 2);

            //Act
            _sut.Add(key3, val3);

            //Assert
            Assert.IsFalse(_sut.Items.ContainsKey(_key));
            Assert.IsFalse(_sut.RequestsCounter.ContainsKey(_key));
            Assert.IsTrue(_sut.Items.ContainsKey(key2));
            Assert.IsTrue(_sut.RequestsCounter.ContainsKey(key2));
            Assert.IsTrue(_sut.Items.ContainsKey(key3));
            Assert.IsTrue(_sut.RequestsCounter.ContainsKey(key3));
        }

        [Test]
        public void RemoveFromItemsAndCounterOnRemove()
        {
            //Arrange
            _sut.Items.TryAdd(_key, _val);
            _sut.RequestsCounter.TryAdd(_key, _val);

            Assert.IsTrue(_sut.Items.ContainsKey(_key));
            Assert.IsTrue(_sut.RequestsCounter.ContainsKey(_key));

            //Act
            _sut.Remove(_key);

            //Assert
            Assert.IsFalse(_sut.Items.ContainsKey(_key));
            Assert.IsFalse(_sut.RequestsCounter.ContainsKey(_key));
        }

        [Test]
        public void ReturnValueIfExists_OnTryGet()
        {
            //Arrange
            _sut.Items.TryAdd(_key, _val);
            _sut.RequestsCounter.TryAdd(_key, _val);
            Assert.IsTrue(_sut.Items.ContainsKey(_key));
            Assert.IsTrue(_sut.RequestsCounter.ContainsKey(_key));

            //Act
            var result = _sut.TryGet(_key);

            //Assert
            Assert.AreEqual(_val, result);
        }

        [Test]
        public void ReturnDefaultValueIfNotExists_OnTryGet()
        {
            //Arrange
            Assert.IsFalse(_sut.Items.ContainsKey(_key));
            Assert.IsFalse(_sut.RequestsCounter.ContainsKey(_key));

            //Act
            var result = _sut.TryGet(_key);

            //Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void IncrementCounterIfExists_OnTryGet()
        {
            //Arrange
            _sut.Items.TryAdd(_key, _val);
            _sut.RequestsCounter.TryAdd(_key, _val);
            Assert.IsTrue(_sut.Items.ContainsKey(_key));
            Assert.IsTrue(_sut.RequestsCounter.ContainsKey(_key));

            var count = _sut.RequestsCounter[_key];

            //Act
            var result = _sut.TryGet(_key);

            //Assert
            Assert.AreEqual(++count, _sut.RequestsCounter[_key]);
        }
    }
}