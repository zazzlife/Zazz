﻿using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class AlbumServiceShould
    {
        private Mock<IUoW> _uoW;
        private AlbumService _sut;
        private Album _album;
        private int _userId;

        [SetUp]
        public void Init()
        {
            _uoW = new Mock<IUoW>();
            _sut = new AlbumService(_uoW.Object);
            _album = new Album();
            _userId = 12;

            _uoW.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }

        [Test]
        public async Task InsertAndSave_OnCreateAlbum()
        {
            //Arrange
            _uoW.Setup(x => x.AlbumRepository.InsertGraph(_album));

            //Act
            await _sut.CreateAlbumAsync(_album);

            //Assert
            _uoW.Verify(x => x.AlbumRepository.InsertGraph(_album), Times.Once());
            _uoW.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task ShouldThrowIfAlbumIdIs0_OnUpdateAlbum()
        {
            //Arrange
            //Act
            try
            {
                await _sut.UpdateAlbumAsync(_album, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (ArgumentException)
            {
            }
        }


    }
}