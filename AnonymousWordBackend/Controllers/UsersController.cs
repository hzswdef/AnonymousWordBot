using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.ControllerParams;
using AnonymousWordBackend.Dto;
using AnonymousWordBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Controllers;

[ApiController]
[Route("/api/user")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly DatabaseContext _databaseContext;

    private const string LinkCharactersPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public UsersController(ILogger<UsersController> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    /// <summary>
    /// Get User by ID.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <returns>User.</returns>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> User(long id)
    {
        User? user = await _databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.Id == id);
        
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    
    /// <summary>
    /// Get User by filters.
    /// </summary>
    /// <param name="userFilters">Filters.</param>
    /// <returns>User.</returns>
    [HttpGet]
    public async Task<IActionResult> UserByFilters([FromQuery] UserFilters userFilters)
    {
        IQueryable<User> query = _databaseContext.Users;

        if (userFilters.Id != null)
            query = query.Where(user => user.Id == userFilters.Id);
        if (userFilters.TelegramId != null)
            query = query.Where(user => user.TelegramId == userFilters.TelegramId);
        if (userFilters.Link != null)
            query = query.Where(user => user.Link == userFilters.Link);
        if (userFilters.RegisteredAt != null)
            query = query.Where(user => user.RegisteredAt == userFilters.RegisteredAt);

        User? user = await query.FirstOrDefaultAsync();
        
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    
    /// <summary>
    /// Get Anonymous Message Author by the send Message.
    /// </summary>
    /// <param name="messageId">Recipient Message ID.</param>
    /// <returns>Author of the message.</returns>
    [HttpGet("author/{messageId:long}")]
    public async Task<IActionResult> GetAuthor(long? messageId)
    {
        Message? message = await _databaseContext
            .Messages
            .FirstOrDefaultAsync(message => message.RecipientChatMessageId == messageId);

        if (message == null)
            return NotFound();
        
        User? user = await _databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.TelegramId == message.AuthorId);
        
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }

    /// <summary>
    /// Patch User.
    /// </summary>
    /// <param name="telegramId">User's Telegram ID.</param>
    /// <param name="data">User Data to patch.</param>
    /// <returns>Created User.</returns>
    [HttpPatch("{telegramId:long}")]
    public async Task<IActionResult> PatchUser(long telegramId, PatchUser data)
    {
        User? user = await _databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.TelegramId == telegramId);

        if (user == null)
            return NotFound();
        
        if (data.Link != null)
        {
            Regex pattern = new (@"^[a-zA-Z0-9_]{6,32}$");
            
            if (data.Link == "del")
                user.Link = null;
            else if (!pattern.IsMatch(data.Link))
                return Conflict("Allowed only english characters, digits, underscore. Allowed length is from 6 to 32 characters.");
            else
            {
                User? userWithSameLink = await _databaseContext
                    .Users
                    .FirstOrDefaultAsync(u => u.Link == data.Link);

                if (userWithSameLink != null)
                    return Conflict("Link already used.");
                
                user.Link = data.Link;
            }
        }
        
        if (data.WelcomeMessage != null)
        {
            Regex pattern = new (@"^.{6,256}$");
            
            if (data.WelcomeMessage == "clear")
                user.WelcomeMessage = null;
            else if (!pattern.IsMatch(data.WelcomeMessage))
                return Conflict("WelcomeMessage allowed length is from 6 to 256 characters.");
            else
                user.WelcomeMessage = data.WelcomeMessage;
        }

        _databaseContext.Users.Update(user);
        await _databaseContext.SaveChangesAsync();
        
        return Ok(user);
    }
    
    /// <summary>
    /// Create User by Telegram ID.
    /// </summary>
    /// <param name="telegramId">User Telegram ID.</param>
    /// <returns>Created User.</returns>
    [HttpPut("{telegramId:long}")]
    public async Task<IActionResult> CreateUser(long telegramId)
    {
        User user = new()
        {
            TelegramId = telegramId,
            Link = RandomNumberGenerator.GetString(
                choices: LinkCharactersPool,
                length: 16
            ),
            WelcomeMessage = null,
            RegisteredAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
        };

        _databaseContext.Users.Add(user);
        await _databaseContext.SaveChangesAsync();
        
        return Ok(user);
    }
}
