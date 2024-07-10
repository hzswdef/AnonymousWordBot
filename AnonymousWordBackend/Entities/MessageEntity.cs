using System.Text.RegularExpressions;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.Extensions;
using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Entities;

public class MessageAuthorNotFound(string message) : Exception(message);
// public class UserEntityInvalidWelcomeMessageException(string message) : Exception(message);

public partial class MessageEntity
{
    private readonly DatabaseContext _databaseContext;
    private readonly MessageModel _messageModel;
    public long Id { get; }
    public long AuthorChatMessageId { get; }
    public long RecipientChatMessageId { get; }
    public long StorageMessageId { get; }
    public long AuthorId { get; }
    public long RecipientId { get; }
    public string? Body { get; }
    public long AuthoredOn { get; }
    
    public MessageEntity(DatabaseContext databaseContext, MessageModel messageModel)
    {
        _databaseContext = databaseContext;
        _messageModel = messageModel;
        Id = messageModel.Id;
        AuthorChatMessageId = messageModel.AuthorChatMessageId;
        RecipientChatMessageId = messageModel.RecipientChatMessageId;
        StorageMessageId = messageModel.StorageMessageId;
        AuthorId = messageModel.AuthorId;
        RecipientId = messageModel.RecipientId;
        Body = messageModel.Body;
        AuthoredOn = messageModel.AuthoredOn;
    }

    public async Task<UserEntity> GetAuthor()
    {
        UserModel? userModel = await _databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.Id == AuthorId);

        if (userModel == null)
            throw new MessageAuthorNotFound("Message Author not found.");

        return new UserEntity(_databaseContext, userModel);
    }
    
    public async Task<UserEntity> GetRecipient()
    {
        UserModel? userModel = await _databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.Id == RecipientId);

        if (userModel == null)
            throw new MessageAuthorNotFound("Message Recipient not found.");

        return new UserEntity(_databaseContext, userModel);
    }
}