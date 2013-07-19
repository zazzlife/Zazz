﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPostService
    {
        Post GetPost(int postId);

        void NewPost(Post post, IEnumerable<int> categories);

        void EditPost(int postId, string newText, IEnumerable<int> categories, int currentUserId);

        void RemovePost(int postId, int currentUserId);
    }
}