using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseAPIController
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepo, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<ActionResult<PageList<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUser = await _userRepo.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = currentUser.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }

            var users = await _userRepo.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new
            PaginationHeader(users.CurretPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUserByUsername(string username)
        {
            var user = await _userRepo.GetMemberAsync(username);

            if (user is null) return NotFound();

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            if (user is null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);

            if (await _userRepo.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            if (user is null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error is not null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                IsMain = (user.Photos.Count == 0)
            };

            user.Photos.Add(photo);

            if (await _userRepo.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUserByUsername),
                        new { username = user.UserName }, _mapper.Map<PhotoDTO>(photo));
            }

            else return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            if (user is null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo is null) return NotFound();

            if (photo.IsMain) return BadRequest("This is already you main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain is not null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _userRepo.SaveAllAsync()) return NoContent();


            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepo.GetUserByUsernameAsync(User.GetUsername());

            if (user is null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo is null) return NotFound();

            if (photo.IsMain) return BadRequest("Main photo cannot be deleted");

            if (photo.PublicId is not null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error is not null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepo.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}