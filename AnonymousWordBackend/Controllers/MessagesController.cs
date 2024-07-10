using System.Runtime.InteropServices;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.ControllerParams;
using AnonymousWordBackend.Dto;
using AnonymousWordBackend.Entities;
using AnonymousWordBackend.Models;
using AnonymousWordBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnonymousWordBackend.Controllers;

[ApiController]
[Route("/api/message")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly MessageService _messageService;

    public MessagesController(
        ILogger<UsersController> logger,
        DatabaseContext databaseContext,
        MessageService messageService)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _messageService = messageService;
    }

    /// <summary>
    /// Get Message by ID.
    /// </summary>
    /// <param name="id">Message ID.</param>
    /// <returns>Message.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> MessageById(int id)
    {
        MessageEntity? message = await _messageService.LoadById(id);
        
        if (message == null)
            return NotFound();
        
        return Ok(message);
    }

    /// <summary>
    /// Get Message by filters.
    /// </summary>
    /// <param name="messageFilters">Filters.</param>
    /// <returns>Message.</returns>
    [HttpGet]
    public async Task<IActionResult> MessageByFilters([FromQuery] MessageFilters messageFilters)
    {
        // Convert a userFilters variable object to the dictionary to load the User by given filters.
        Dictionary<string, object>? properties = JObject
            .FromObject(messageFilters, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore })
            .ToObject<Dictionary<string, object>>();

        if (properties == null)
            return NotFound();

        MessageEntity? message = await _messageService.LoadByProperties(properties);

        if (message == null)
            return NotFound();
        
        return Ok(message);
    }
    
    /// <summary>
    /// Create Message.
    /// </summary>
    /// <param name="data">Message data.</param>
    /// <returns>Created Message.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateMessage([FromBody] PostMessage data)
    {
        MessageModel messageModel = new()
        {
            AuthorChatMessageId = data.AuthorChatMessageId,
            RecipientChatMessageId = data.RecipientChatMessageId,
            StorageMessageId = data.StorageMessageId,
            AuthorId = data.AuthorId,
            RecipientId = data.RecipientId,
            AuthoredOn = DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        if (data.Body != null)
            messageModel.Body = data.Body;

        _databaseContext.Messages.Add(messageModel);
        await _databaseContext.SaveChangesAsync();
        
        return Ok(messageModel);
    }
}
