using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    // I'm going to leave this integration test here because it's very fast.

    [TestFixture]
    public class FileServiceShould
    {
        private FileService _sut;
        private readonly string _tempPath;
        private DirectoryInfo _tempDir;

        public FileServiceShould()
        {
            _tempPath = Path.GetTempPath() + "ZAZZ_TESTS";
        }

        [SetUp]
        public void Init()
        {
            _sut = new FileService();
            _tempDir = Directory.CreateDirectory(_tempPath);
        }

        [TestCase(@"C:\Pictures\123\Test\1234.jpg", @"C:\Pictures\123\Test")]
        [TestCase(@"C:\Pictures\133\Test\1232\123455.jpg", @"C:\Pictures\133\Test\1232")]
        [TestCase(@"C:\Pictures\123\Test\232\123422.jpg", @"C:\Pictures\123\Test\232")]
        public void ReturnCorrectPath_OnRemoveFileNameFromPath(string path, string expected)
        {
            //Arrange
            //Act
            var result = _sut.RemoveFileNameFromPath(path);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CreateDirectoryWhenNotExists_OnCreateDirIfNotExists([Values(@"\Test2",
                                             @"\Test3\tt",
                                             @"\Test3\tt\1234")] string dir)
        {
            //Arrange
            var dirPath = _tempPath + dir;

            //Act
            _sut.CreateDirIfNotExists(dirPath);

            //Assert
            Assert.IsTrue(Directory.Exists(dirPath));
        }

        [Test]
        public void NotDeleteOrThrowWhenDirectoryExists_OnCreateDirIfNotExists(
                                                                            [Values(@"\Test2",
                                                                                @"\Test3\tt",
                                                                                @"\Test3\tt\1234")] string dir)
        {
            //Arrange
            var dirPath = _tempPath + dir;
            Directory.CreateDirectory(dirPath);

            //Act
            _sut.CreateDirIfNotExists(dirPath);

            //Assert
            Assert.IsTrue(Directory.Exists(dirPath));
        }

        [Test]
        public async Task SaveFileUsingStreamWhenFileNotExists_OnSaveFile()
        {
            //Arrange
            var buffer = new byte[] {1, 23, 44, 55, 56, 77, 88};
            var fileName = "sample";
            var fullFilePath = _tempPath + @"\" + fileName;

            using (var ms = new MemoryStream())
            {
                ms.Write(buffer, 0, buffer.Length);

                //Act
                await _sut.SaveFileAsync(fullFilePath, ms);

                //Assert
                Assert.IsTrue(File.Exists(fullFilePath));
                using (var file = File.Open(fullFilePath, FileMode.Open))
                {
                    Assert.AreEqual(buffer.Length, file.Length);
                }
            }
        }

        [Test]
        public async Task SaveFileUsingByteArrayWhenFileNotExists_OnSaveFile()
        {
            //Arrange
            var buffer = new byte[] { 1, 23, 44, 55, 56, 77, 88 };
            var fileName = "sample";
            var fullFilePath = _tempPath + @"\" + fileName;

            //Act
            await _sut.SaveFileAsync(fullFilePath, buffer);

            //Assert
            Assert.IsTrue(File.Exists(fullFilePath));
            using (var file = File.Open(fullFilePath, FileMode.Open))
            {
                Assert.AreEqual(buffer.Length, file.Length);
            }
        }

        [Test]
        public async Task SaveFileUsingStreamWhenFileExists_OnSaveFile()
        {
            //Arrange
            var buffer = new byte[] { 1, 23, 44, 55, 56, 77, 88 };
            var fileName = "sample";
            var fullFilePath = _tempPath + @"\" + fileName;

            using (var file = File.Create(fullFilePath))
            {}

            using (var ms = new MemoryStream())
            {
                ms.Write(buffer, 0, buffer.Length);

                //Act
                await _sut.SaveFileAsync(fullFilePath, ms);

                //Assert
                Assert.IsTrue(File.Exists(fullFilePath));
                using (var file = File.Open(fullFilePath, FileMode.Open))
                {
                    Assert.AreEqual(buffer.Length, file.Length);
                }
            }
        }

        [Test]
        public async Task SaveFileUsingByteArrayWhenFileExists_OnSaveFile()
        {
            //Arrange
            var buffer = new byte[] { 1, 23, 44, 55, 56, 77, 88 };
            var fileName = "sample";
            var fullFilePath = _tempPath + @"\" + fileName;
            
            using (var file = File.Create(fullFilePath))
            { }

            //Act
            await _sut.SaveFileAsync(fullFilePath, buffer);

            //Assert
            Assert.IsTrue(File.Exists(fullFilePath));
            using (var file = File.Open(fullFilePath, FileMode.Open))
            {
                Assert.AreEqual(buffer.Length, file.Length);
            }
        }

        [Test]
        public void ShouldRemoveFile_OnRemoveFile()
        {
            //Arrange
            var fileName = "sample";
            var fullFilePath = _tempPath + @"\" + fileName;

            using (var file = File.Create(fullFilePath))
            { }

            Assert.IsTrue(File.Exists(fullFilePath));

            //Act
            _sut.RemoveFile(fullFilePath);

            //Assert
            Assert.IsFalse(File.Exists(fullFilePath));
        }


        [Test]
        public void NotThrowWhenFileNotExists_OnRemoveFile()
        {
            //Arrange
            var fileName = "sample";
            var fullFilePath = _tempPath + @"\" + fileName;

            Assert.IsFalse(File.Exists(fullFilePath));

            //Act
            _sut.RemoveFile(fullFilePath);

            //Assert
            Assert.IsFalse(File.Exists(fullFilePath));
        }

        [TearDown]
        public void Cleanup()
        {
            _tempDir.Delete(true);
        }
    }
}