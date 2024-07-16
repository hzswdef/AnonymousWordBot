using System.Text.RegularExpressions;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.Extensions;
using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Entities;

public class UserEntityInvalidLinkException(string message) : Exception(message);
public class UserEntityInvalidWelcomeMessageException(string message) : Exception(message);

public partial class UserEntity
{
    private readonly DatabaseContext _databaseContext;
    private readonly UserModel _userModel;
    public long Id { get; }
    public long TelegramId { get; }
    public string? Link { get; set; }
    public string? WelcomeMessage { get; set; }
    public Roles Roles { get; set; }
    public long RegisteredAt { get; }
    
    public UserEntity(DatabaseContext databaseContext, UserModel userModel)
    {
        _databaseContext = databaseContext;
        _userModel = userModel;
        Id = userModel.Id;
        TelegramId = userModel.TelegramId;
        Link = userModel.Link;
        WelcomeMessage = userModel.WelcomeMessage;
        Roles = userModel.Roles;
        RegisteredAt = userModel.RegisteredAt;
    }

    /// <summary>
    /// Set User's unique "Link".
    /// </summary>
    /// <param name="link">User's Link.</param>
    /// <returns>UserEntity</returns>
    /// <exception cref="UserEntityInvalidLinkException"></exception>
    public async Task<UserEntity> SetLink(string? link)
    {
        Regex pattern = LinkRegex();
        
        if (link == "del")
            link = null;
        else if (link != null && !pattern.IsMatch(link))
            throw new UserEntityInvalidLinkException("Invalid Link. Allowed only english characters, digits, underscore. Allowed length is from 6 to 32 characters.");

        UserModel? userWithSameLink = await _databaseContext
            .Users
            .FirstOrDefaultAsync(u => u.Link == link);
        if (userWithSameLink != null)
            throw new UserEntityInvalidLinkException("Link already used.");
            
        Link = link;
        _userModel.Link = link;
        
        _databaseContext.Users.Update(_userModel);
        await _databaseContext.SaveChangesAsync();
        
        return this;
    }
    
    /// <summary>
    /// Set User's "Welcome Message".
    /// </summary>
    /// <param name="welcomeMessage">Welcome Message.</param>
    /// <returns>UserEntity</returns>
    /// <exception cref="UserEntityInvalidLinkException"></exception>
    public async Task<UserEntity> SetWelcomeMessage(string? welcomeMessage)
    {
        Regex pattern = WelcomeMessageRegex();
        
        if (welcomeMessage == "clear")
            welcomeMessage = null;
        else if (welcomeMessage != null && !pattern.IsMatch(welcomeMessage))
            throw new UserEntityInvalidWelcomeMessageException("Invalid WelcomeMessage. Allowed length is from 6 to 256 characters.");
            
        WelcomeMessage = welcomeMessage;
        _userModel.WelcomeMessage = welcomeMessage;

        _databaseContext.Users.Update(_userModel);
        await _databaseContext.SaveChangesAsync();
        
        return this;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_]{6,32}$")]
    private static partial Regex LinkRegex();
    
    [GeneratedRegex(@"^.{6,256}$")]
    private static partial Regex WelcomeMessageRegex();
}