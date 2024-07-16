using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Entities;

public class BanListIssuerNotFound(string message) : Exception(message);
public class BanListBannedNotFound(string message) : Exception(message);
public class BanListUserIsAlreadyBanned() : Exception("User is already banned.");
public class BanListUserIsAlreadyUnbanned() : Exception("User is already unbanned.");

public class BanListEntity
{
    private readonly DatabaseContext _databaseContext;
    private readonly BanListModel _banListModel;
    public long Id { get; }
    public bool IsBanned { get; set; }
    public long MessageId { get; }
    public long IssuerId { get; }
    public long BannedId { get; }
    public long BannedOn { get; }
    
    public BanListEntity(DatabaseContext databaseContext, BanListModel banListModel)
    {
        _databaseContext = databaseContext;
        _banListModel = banListModel;
        Id = banListModel.Id;
        IsBanned = banListModel.IsBanned;
        MessageId = banListModel.MessageId;
        IssuerId = banListModel.IssuerId;
        BannedId = banListModel.BannedId;
        BannedOn = banListModel.BannedOn;
    }

    public async Task<UserEntity> GetIssuer()
    {
        UserModel? userModel = await _databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.Id == IssuerId);

        if (userModel == null)
            throw new BanListIssuerNotFound("Issuer not found.");

        return new UserEntity(_databaseContext, userModel);
    }
    
    public async Task<UserEntity> GetBanned()
    {
        UserModel? userModel = await _databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.Id == BannedId);

        if (userModel == null)
            throw new BanListBannedNotFound("Banned not found.");

        return new UserEntity(_databaseContext, userModel);
    }
    
    public async Task<BanListEntity> SetBanned()
    {
        if (IsBanned)
        {
            throw new BanListUserIsAlreadyBanned();
        }
        
        IsBanned = true;
        _banListModel.IsBanned = true;
        
        _databaseContext.BanList.Update(_banListModel);
        await _databaseContext.SaveChangesAsync();

        return this;
    }
    
    public async Task<BanListEntity> SetUnbanned()
    {
        if (!IsBanned)
        {
            throw new BanListUserIsAlreadyUnbanned();
        }
        
        IsBanned = false;
        _banListModel.IsBanned = false;
        
        _databaseContext.BanList.Update(_banListModel);
        await _databaseContext.SaveChangesAsync();

        return this;
    }
}