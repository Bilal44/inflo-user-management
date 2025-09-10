using System;
using System.Net;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Exceptions;
using UserManagement.Services.Interfaces;
using UserManagement.Services.Mapping;

namespace UserManagement.Web.Controllers;

public class UserController(
    IUserService userService) : Controller
{
    // Return the list of users, optionally filtered by `IsActive` status.
    public async Task<IActionResult> List(bool? active)
    {
        var users = active.HasValue
            ? await userService.FilterByActiveAsync(active.Value)
            : await userService.GetAllAsync();

        return View(UserMapper.MapToUserModelList(users));
    }

    // Return a single user associated with the provided id.
    public async Task<IActionResult> View(long? id)
    {
        try
        {
            if (id is null)
            {
                AddNotificationPanel("view", "error", id);
                return RedirectToAction(nameof(List));
            }

            var user = await userService.GetByIdAsync(id.Value);
            if (user is null)
            {
                AddNotificationPanel("view", "error", id);
                return RedirectToAction(nameof(List));
            }

            return View(UserMapper.MapToUserModel(user));
        }
        catch (Exception)
        {
            AddNotificationPanel("view", "exception", id);
            return RedirectToAction(nameof(List));
        }
    }

    // View with an empty form to create a new user.
    public IActionResult Add() =>
        View();

    // Create a new user after validating the user model.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(UserModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = UserMapper.MapToUserEntity(model);
            await userService.AddAsync(user);
            AddNotificationPanel("add", "success", user.Id, user);
            return RedirectToAction(nameof(List));
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.Conflict)
        {
            AddNotificationPanel("add", "error", model.Id);
            return View(model);
        }
        catch (Exception)
        {
            AddNotificationPanel("add", "exception", model.Id);
            return View(model);
        }
    }

    // View with a pre-filled form to update a current user.
    public async Task<IActionResult> Edit(long? id)
    {
        try
        {
            if (id is null)
            {
                AddNotificationPanel("view", "error", id);
                return RedirectToAction(nameof(List));
            }

            var user = await userService.GetByIdAsync(id.Value);
            if (user is null)
            {
                AddNotificationPanel("view", "error", id);
                return RedirectToAction(nameof(List));
            }

            return View(UserMapper.MapToUserModel(user));
        }
        catch (Exception)
        {
            AddNotificationPanel("view", "exception", id);
            return RedirectToAction(nameof(List));
        }
    }

    // Update an existing user after validating the user model.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, UserModel model)
    {
        try
        {
            if (id != model.Id)
            {
                AddNotificationPanel("update", "error", id);
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = UserMapper.MapToUserEntity(model);
            await userService.UpdateAsync(user);
            AddNotificationPanel("update", "success", user.Id, user);
            return RedirectToAction(nameof(List));
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.Conflict)
        {
            AddNotificationPanel("update", "error", model.Id);
            return View(model);
        }
        catch (Exception)
        {
            AddNotificationPanel("update", "exception", id);
            return View(model);
        }
    }

    // Read-only view with user details.
    public async Task<IActionResult> Delete(long? id)
    {
        try
        {
            if (id is null)
            {
                AddNotificationPanel("view", "error", id);
                return RedirectToAction(nameof(List));
            }

            var user = await userService.GetByIdAsync(id.Value);
            if (user is null)
            {
                AddNotificationPanel("view", "error", id);
                return RedirectToAction(nameof(List));
            }

            return View(UserMapper.MapToUserModel(user));
        }
        catch (Exception)
        {
            AddNotificationPanel("view", "exception", id);
            return RedirectToAction(nameof(List));
        }
    }

    // Permanently deletes an existing user from the system.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long? id)
    {
        try
        {
            if (id is null)
            {
                AddNotificationPanel("delete", "error", id);
                return View();
            }

            var deleted = await userService.DeleteAsync(id.Value);
            if (deleted != 1)
            {
                AddNotificationPanel("delete", "error", id);
                return View();
            }

            AddNotificationPanel("delete", "success", id.Value);
            return RedirectToAction(nameof(List));
        }
        catch (Exception)
        {
            AddNotificationPanel("delete", "exception", id);
            return RedirectToAction(nameof(Edit), id);
        }
    }

    // Display the success/failure status in a partial view on the page for a better user experience.
    private void AddNotificationPanel(
        string action,
        string status,
        long? userId,
        User? user = null)
    {
        var message = status switch
        {
            "success" => action switch
            {
                "add" =>
                    $"Successfully added <b>{user!.Forename} {user.Surname}</b> as a new <b>{(user.IsActive ? "Active" : "Inactive")}</b> user.",
                "update" => $"Successfully updated user details for <b>{user!.Forename} {user.Surname}</b>.",
                "delete" => $"Successfully deleted user with id [{userId}].",
                _ => string.Empty
            },

            "error" => action switch
            {
                "view" => $"Failed to find a user with id [{userId}].",
                "add" or "update" => "A user with this email address already exists.",
                _ => $"Failed to {action} user."
            },

            "exception" =>
                "An unexpected error occurred. Please try again. If the problem persists, please contact our support team.",
            _ => "Unknown status."
        };

        TempData[status] = message;
    }
}
