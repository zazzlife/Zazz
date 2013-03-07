using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class PhotoServiceShould
    {
        private Mock<IUoW> _uow;
        private Mock<IFileService> _fs;
        private PhotoService _sut;
        private string _tempRootPath;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _fs = new Mock<IFileService>();
            _tempRootPath = Path.GetTempPath();
            _sut = new PhotoService(_uow.Object, _fs.Object, _tempRootPath);
        }

        [TestCase("/picture/user/12/2/333.jpg", 12, 2, 333)]
        [TestCase("/picture/user/800/9000/1203200.jpg", 800, 9000, 1203200)]
        [TestCase("/picture/user/102/20/3330.jpg", 102, 20, 3330)]
        public void GenerateCorrectPath_OnGeneratePhotoUrl(string expected, int userId, int albumId, int photoId)
        {
            //Arrange
            //Act
            var result = _sut.GeneratePhotoUrl(userId, albumId, photoId);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(12, 2, 333)]
        [TestCase(800, 9000, 1203200)]
        [TestCase(102, 20, 3330)]
        public void GenerateCorrectPath_OnGeneratePhotoFilePath(int userId, int albumId, int photoId)
        {
            //Arrange
            var expected = String.Format(@"{0}\picture\user\{1}\{2}\{3}.jpg", _tempRootPath, userId, albumId, photoId);

            //Act
            var result = _sut.GeneratePhotoFilePath(userId, albumId, photoId);

            //Assert
            Assert.AreEqual(expected, result);
        }


    }
}