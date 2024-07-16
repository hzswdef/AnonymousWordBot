using System.Security.Cryptography;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.ControllerParams;
using AnonymousWordBackend.Dto;
using AnonymousWordBackend.Entities;
using AnonymousWordBackend.Extensions;
using AnonymousWordBackend.Models;
using AnonymousWordBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnonymousWordBackend.Controllers;

[ApiController]
[Route("/api/user")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly UserService _userService;
    private readonly MessageService _messageService;

    private const string LinkCharactersPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public UsersController(
        ILogger<UsersController> logger,
        DatabaseContext databaseContext, 
        UserService userService,
        MessageService messageService)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _userService = userService;
        _messageService = messageService;
    }

    /// <summary>
    /// Get User by ID.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <returns>User.</returns>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> User(long id)
    {
        UserEntity? user = await _userService.LoadById(id);
        
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
        // Convert a userFilters variable object to the dictionary to load the User by given filters.
        Dictionary<string, object>? properties = JObject
            .FromObject(userFilters, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore })
            .ToObject<Dictionary<string, object>>();

        if (properties == null)
            return NotFound();

        UserEntity? user = await _userService.LoadByProperties(properties);
        
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
    public async Task<IActionResult> GetAuthor(long messageId)
    {
        Dictionary<string, object> messageProperties = new () { {
            "RecipientChatMessageId", messageId
        } };
        MessageEntity? message = await _messageService.LoadByProperties(messageProperties);

        if (message == null)
            return NotFound();

        Dictionary<string, object> userProperties = new () {
        {
            "TelegramId", message.AuthorId
        } };
        UserEntity? user = await _userService.LoadByProperties(userProperties);
        
        return user == null
            ? NotFound()
            : Ok(user);
    }
    
    /// <summary>
    /// Get Anonymous Message Author by Message from the storage channel.
    /// </summary>
    /// <param name="messageId">Recipient Message ID.</param>
    /// <returns>Author of the message.</returns>
    [HttpGet("author_from_storage/{messageId:long}")]
    public async Task<IActionResult> GetAuthorFromStorage(long messageId)
    {
        Dictionary<string, object> messageProperties = new () { {
            "StorageMessageId", messageId
        } };
        MessageEntity? message = await _messageService.LoadByProperties(messageProperties);

        if (message == null)
            return NotFound();

        Dictionary<string, object> userProperties = new () {
        {
            "TelegramId", message.AuthorId
        } };
        UserEntity? user = await _userService.LoadByProperties(userProperties);
        
        return user == null
            ? NotFound()
            : Ok(user);
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
        Dictionary<string, object> userProperties = new () {
        {
            "TelegramId", telegramId
        } };
        UserEntity? user = await _userService.LoadByProperties(userProperties);
        
        if (user == null)
            return NotFound();

        if (data.Link != null)
            user = await user.SetLink(data.Link);
        if (data.WelcomeMessage != null)
            user = await user.SetWelcomeMessage(data.WelcomeMessage);
        
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
        UserModel userModel = new()
        {
            TelegramId = telegramId,
            Link = RandomNumberGenerator.GetString(
                choices: LinkCharactersPool,
                length: 16
            ),
            WelcomeMessage = null,
            Roles = Roles.User,
            RegisteredAt = DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        _databaseContext.Users.Add(userModel);
        await _databaseContext.SaveChangesAsync();
        
        return Ok(userModel);
    }
}
