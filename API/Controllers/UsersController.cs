using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseAPIController
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepo, IMapper mapper)
        {
            _mapper = mapper;
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            var users = await _userRepo.GetMembersAsync();

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
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepo.GetUserByUsernameAsync(username);

            if (user is null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);

            if (await _userRepo.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}