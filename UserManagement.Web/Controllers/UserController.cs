using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Data.Entities.Enums;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Interfaces;
using UserManagement.Services.Mapping;

namespace UserManagement.Web.Controllers;

public class UserController(
    IUserService userService,
    ILogService logService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> List(bool? active)
    {
        var users = active.HasValue
            ? await userService.FilterByActiveAsync(active.Value)
            : await userService.GetAllAsync();

        return View(UserMapper.MapToUserModelList(users));
    }

    // GET:
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

            await AddLogEntry(ActionType.AddUser, user.Id);

            return View(UserMapper.MapToUserModel(user));
        }
        catch (Exception)
        {
            AddNotificationPanel("view", "exception", id);
            return RedirectToAction(nameof(List));
        }
    }

    // GET: u/Create
    public IActionResult Add() =>
        View();

    // POST: u/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            await AddLogEntry(ActionType.AddUser, user.Id);
            AddNotificationPanel("add", "success", user.Id, user);
            return RedirectToAction(nameof(List));
        }
        catch (Exception)
        {
            AddNotificationPanel("add", "exception", model.Id);
            return View(model);
        }
    }

    // GET: u/Edit/5
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

    // POST: u/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            await AddLogEntry(ActionType.UpdateUser, user.Id);
            AddNotificationPanel("update", "success", user.Id);
            return RedirectToAction(nameof(List));
        }
        catch (Exception)
        {
            AddNotificationPanel("update", "exception", id);
            return View(model);
        }
    }

    // GET: u/Delete/5
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

    // POST: u/Delete/5
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

            await AddLogEntry(ActionType.AddUser, id.Value);
            AddNotificationPanel("delete", "success", id.Value);
            return RedirectToAction(nameof(List));
        }
        catch (Exception)
        {
            AddNotificationPanel("delete", "exception", id);
            return RedirectToAction(nameof(Edit), id);
        }
    }

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
                "add" => $"Successfully added '{user!.Forename} {user.Surname}' as a new {(user.IsActive ? "Active" : "Inactive")} user.",
                "update" => $"Successfully updated user with id [{userId}].",
                "delete" => $"Successfully deleted user with id [{userId}].",
                _ => string.Empty
            },

            "error" => action switch
            {
                "view" => $"Failed to find a user with id [{userId}].",
                _ => $"Failed to {action} user."
            },

            "exception" => "An unexpected error occurred. Please try again. If the problem persists, please contact our support team.",
            _ => "Unknown status."
        };

        TempData[status] = message;
    }

    private async Task AddLogEntry(
        ActionType actionType,
        long userId)
    {
        var actions = new ReadOnlyDictionary<ActionType, string>(
            new Dictionary<ActionType, string>
            {
                { ActionType.ViewUser, "Viewed" },
                { ActionType.AddUser, "Added" },
                { ActionType.UpdateUser, "Updated" },
                { ActionType.DeleteUser, "Deleted" }
            }
        );

        var log = new LogModel
        {
            UserId = userId,
            ActionType = actionType,
            Details = $"{actions.GetValueOrDefault(actionType)} user with id [{userId}]",
        };
        await logService.AddAsync(LogMapper.MapToLogEntity(log));
    }
}
