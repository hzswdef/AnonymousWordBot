using System.Runtime.InteropServices;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.ControllerParams;
using AnonymousWordBackend.Dto;
using AnonymousWordBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Controllers;

[ApiController]
[Route("/api/message")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly DatabaseContext _databaseContext;

    public MessagesController(ILogger<UsersController> logger, DatabaseContext databaseContext)
    {
        _logger = logger;
        _databaseContext = databaseContext;
    }

    /// <summary>
    /// Get Message by ID.
    /// </summary>
    /// <param name="id">Message ID.</param>
    /// <returns>Message.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> MessageById(int id)
    {
        Message? message = await _databaseContext
            .Messages
            .FirstOrDefaultAsync(message => message.Id == id);

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
        IQueryable<Message> query = _databaseContext.Messages;

        if (messageFilters.Id != null)
            query = query.Where(message => message.Id == messageFilters.Id);
        if (messageFilters.AuthorChatMessageId != null)
            query = query.Where(message => message.AuthorChatMessageId == messageFilters.AuthorChatMessageId);
        if (messageFilters.RecipientChatMessageId != null)
            query = query.Where(message => message.RecipientChatMessageId == messageFilters.RecipientChatMessageId);
        if (messageFilters.StorageMessageId != null)
            query = query.Where(message => message.StorageMessageId == messageFilters.StorageMessageId);
        if (messageFilters.AuthorId != null)
            query = query.Where(message => message.AuthorId == messageFilters.AuthorId);
        if (messageFilters.RecipientId != null)
            query = query.Where(message => message.RecipientId == messageFilters.RecipientId);
        if (messageFilters.Body != null)
            query = query.Where(message => message.Body == messageFilters.Body);
        if (messageFilters.AuthoredOn != null)
            query = query.Where(message => message.AuthoredOn == messageFilters.AuthoredOn);

        Message? message = await query.FirstOrDefaultAsync();

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
        Message message = new()
        {
            AuthorChatMessageId = data.AuthorChatMessageId,
            RecipientChatMessageId = data.RecipientChatMessageId,
            StorageMessageId = data.StorageMessageId,
            AuthorId = data.AuthorId,
            RecipientId = data.RecipientId,
            AuthoredOn = DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        if (data.Body != null)
            message.Body = data.Body;

        _databaseContext.Messages.Add(message);
        await _databaseContext.SaveChangesAsync();
        
        return Ok(message);
    }
}
