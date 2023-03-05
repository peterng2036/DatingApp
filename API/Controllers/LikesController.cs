using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController : BaseAPIController
    {
        private readonly ILikesRepository _likesRepo;
        private readonly IUserRepository _userRepo;
        public LikesController(IUserRepository userRepo, ILikesRepository likesRepo)
        {
            _userRepo = userRepo;
            _likesRepo = likesRepo;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepo.GetUserByUsernameAsync(username);
            var SourceUser = await _likesRepo.GetUserWithLikes(sourceUserId);

            if (likedUser is null) return NotFound();

            if (SourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _likesRepo.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already like this user");

            userLike = new Entities.UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            SourceUser.LikedUsers.Add(userLike);

            if (await _userRepo.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet()]
        public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepo.GetUserLikes(likesParams);
            Response.AddPaginationHeader(
                new PaginationHeader(users.CurretPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}