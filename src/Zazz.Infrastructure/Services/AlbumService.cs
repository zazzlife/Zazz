﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        public AlbumService(IUoW uow, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
        }

        public Album GetAlbum(int albumId, bool includePhotos)
        {
            return _uow.AlbumRepository.GetById(albumId);
        }

        public List<Album> GetUserAlbums(int userId, int skip, int take)
        {
            return _uow.AlbumRepository.GetAll()
                       .Where(a => a.UserId == userId)
                       .OrderBy(a => a.Id)
                       .Skip(skip)
                       .Take(take).ToList();
        }

        public List<Album> GetUserAlbums(int userId)
        {
            return _uow.AlbumRepository.GetAll()
                       .Where(a => a.UserId == userId).ToList();
        }

        public int GetUserAlbumsCount(int userId)
        {
            return _uow.AlbumRepository.GetAll().Count(a => a.UserId == userId);
        }

        public void CreateAlbum(Album album)
        {
            if (album.CreatedDate == default (DateTime))
                album.CreatedDate = DateTime.UtcNow;

            _uow.AlbumRepository.InsertGraph(album);
            // there is a direct call to repository in FacebookService (get page photos)
            _uow.SaveChanges();
        }

        public void UpdateAlbum(int albumId, string newName, int currentUserId)
        {
            var album = _uow.AlbumRepository.GetById(albumId);
            if (album == null)
                throw new NotFoundException();

            if (album.UserId != currentUserId)
                throw new SecurityException();

            album.Name = newName;
            _uow.SaveChanges();
        }

        public void DeleteAlbum(int albumId, int currentUserId)
        {
            var album = _uow.AlbumRepository.GetById(albumId, true);

            if (album == null)
                throw new NotFoundException();

            if (album.UserId != currentUserId)
                throw new SecurityException();

            foreach (var p in album.Photos)
                _photoService.RemovePhoto(p.Id, currentUserId);

            _uow.AlbumRepository.Remove(album);
            _uow.SaveChanges();
        }
    }
}