﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Controllers.Api
{
    public class RegisterController : ApiController
    {
        private readonly IAuthService _authService;
        private readonly IOAuthClientRepository _oauthClientRepository;
        private readonly IOAuthService _oAuthService;
        private readonly IStaticDataRepository _staticDataRepository;

        public RegisterController(IAuthService authService, IOAuthClientRepository oauthClientRepository,
            IOAuthService oAuthService, IStaticDataRepository staticDataRepository)
        {
            _authService = authService;
            _oauthClientRepository = oauthClientRepository;
            _oAuthService = oAuthService;
            _staticDataRepository = staticDataRepository;
        }

        public HttpResponseMessage Post(ApiRegister request)
        {
            if (request == null ||
                String.IsNullOrWhiteSpace(request.Username) ||
                String.IsNullOrWhiteSpace(request.Password) ||
                String.IsNullOrWhiteSpace(request.Email)
                )
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }

            if (request.AccountType == AccountType.Club && String.IsNullOrWhiteSpace(request.ClubName))
                throw new OAuthException(OAuthError.InvalidRequest);

            //authorizing client
            if (Request.Headers.Authorization == null ||
                String.IsNullOrWhiteSpace(Request.Headers.Authorization.Parameter))
                throw new OAuthException(OAuthError.InvalidClient);

            var clientId = Request.Headers.Authorization.Parameter;

            var client = _oauthClientRepository.GetById(clientId);
            if (client == null)
                throw new OAuthException(OAuthError.InvalidClient);

            User user;
            if (request.AccountType == AccountType.User)
            {

                user = new User
                       {
                           AccountType = AccountType.User,
                           Email = request.Email,
                           IsConfirmed = false,
                           JoinedDate = DateTime.UtcNow,
                           LastActivity = DateTime.UtcNow,
                           Username = request.Username,
                           tagline = request.TagLine,
                           UserDetail = new UserDetail
                                        {
                                            FullName = request.FullName,
                                            Gender = request.Gender,
                                            Birthdate = request.Birthdate,
                                            UserType = request.UserType,
                                            MajorId = request.MajorId,
                                            PromoterType = request.PromoterType
                                        },
                           Preferences = new UserPreferences
                                         {
                                             SendSyncErrorNotifications = true,
                                             SyncFbEvents = true,
                                             SyncFbImages = false,
                                             SyncFbPosts = false,
                                         }
                       };
                if (user.UserDetail.UserType == UserType.Promoter)
                {
                    user.UserDetail.IsPromoter = true;
                }
            }
            else
            {
                user = new User
                       {
                           AccountType = AccountType.Club,
                           Email = request.Email,
                           IsConfirmed = false,
                           JoinedDate = DateTime.UtcNow,
                           LastActivity = DateTime.UtcNow,
                           Username = request.Username,
                           ClubDetail = new ClubDetail
                                        {
                                            Address = request.ClubAddress,
                                            ClubName = request.ClubName,
                                            ClubType = request.ClubType
                                        },
                           Preferences = new UserPreferences
                                         {
                                             SendSyncErrorNotifications = true,
                                             SyncFbEvents = true,
                                             SyncFbImages = true,
                                             SyncFbPosts = true,
                                         }
                       };
            }

            try
            {
                var u = _authService.Register(user, request.Password, true);
                var scope = _staticDataRepository.GetOAuthScopes()
                    .FirstOrDefault(s => s.Name.Contains("full"));

                var oauthCreds = _oAuthService.CreateOAuthCredentials(u, client, new List<OAuthScope> { scope });

                var token = new OAuthAccessTokenResponse
                       {
                           UserId = user.Id,
                           AccessToken = oauthCreds.AccessToken.ToJWTString(),
                           ExpiresIn = 60*60,
                           RefreshToken = oauthCreds.RefreshToken.ToJWTString(),
                           TokenType = "Bearer"
                       };

                var response = Request.CreateResponse(HttpStatusCode.Created, token);
                return response;
            }
            catch (InvalidEmailException)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "invalid email");
            }
            catch (PasswordTooLongException)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "password too long");
            }
            catch (UsernameExistsException)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "username exists");
            }
            catch (EmailExistsException)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "email exists");
            }
            catch (InvalidBirthdateException)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "invalid birthdate");
            }
            catch (InvalidUserWithPromoterType)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "if userType is 1 (User), request must not have promoterType");
            }
            catch (InvalidPromoterWithMajorId)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "if userType is 2 (Promoter), request must not have majorId");
            }
            catch (InvalidUserType)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "invalid userType");
            }
            catch (InvalidPromoterType)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "invalid promoterType");
            }
            catch (InvalidMajorId)
            {
                throw new OAuthException(OAuthError.InvalidRequest, "invalid majorId");
            }
        }
    }
}
