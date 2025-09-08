using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Filters;
using UserManagement.Services.Interfaces;
using UserManagement.Services.Mapping;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace UserManagement.Api.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/users")]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [EnableRateLimiting("sliding")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of users, can be filtered by their active status.
        /// </summary>
        /// <param name="active">Optional: Filters users by their active status.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A list of user models.</returns>
        [ProducesResponseType(typeof(List<UserModel>), Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status500InternalServerError)]
        [HttpGet]
        public async Task<ActionResult<List<UserModel>>> GetUsers(
            bool? active,
            CancellationToken cancellationToken
        )
        {
            var users = active.HasValue
                ? await userService.FilterByActiveAsync(active.Value, cancellationToken)
                : await userService.GetAllAsync(cancellationToken);
            return Ok(UserMapper.MapToUserModelList(users));
        }

        /// <summary>
        /// Retrieves a single user by their unique ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The user matching the provided ID.</returns>
        [ProducesResponseType(typeof(UserModel), Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status500InternalServerError)]
        [HttpGet("{id:long}")]
        public async Task<ActionResult> GetUser(long id)
        {
            var user = await userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(UserMapper.MapToUserModel(user));
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="user">The updated user data.</param>
        /// <returns>No content.</returns>
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status500InternalServerError)]
        [Consumes("application/json")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateUser(long id, UserModel user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            await userService.UpdateAsync(UserMapper.MapToUserEntity(user));
            return NoContent();
        }

        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="user">New user data.</param>
        /// <returns>The details of newly created user.</returns>
        [ProducesResponseType(typeof(UserModel), Status201Created)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status500InternalServerError)]
        [Consumes("application/json")]
        [HttpPost]
        public async Task<ActionResult<User>> AddUser(UserModel user)
        {
            await userService.AddAsync(UserMapper.MapToUserEntity(user));
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>No content.</returns>
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status500InternalServerError)]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await userService.GetByIdAsync(id);
            if (user is null)
                return NotFound();

            var deleted = await userService.DeleteAsync(id);

            if (deleted == 1)
                return NoContent();

            return new ObjectResult(
                new { UserId = id, Message = "Failed to delete user.", Timestamp = DateTime.UtcNow })
                {
                    StatusCode = Status500InternalServerError
                };
        }
    }
}
