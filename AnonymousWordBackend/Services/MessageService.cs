using System.Linq.Expressions;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.Entities;
using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Services;

public class MessageService (DatabaseContext databaseContext)
{
    public async Task<MessageEntity?> LoadById(long id)
    {
        MessageModel? message = await databaseContext
            .Messages
            .FirstOrDefaultAsync(user => user.Id == id);

        return message == null
            ? null
            : new MessageEntity(databaseContext, message);
    }
    
    public async Task<MessageEntity?> LoadByProperties(Dictionary<string, object> properties)
    {
        IQueryable<MessageModel> query = databaseContext.Messages.AsQueryable();

        foreach (var property in properties)
            query = query.Where(GetExpression(property.Key, property.Value));

        MessageModel? message = await query.FirstOrDefaultAsync();

        return message == null
            ? null
            : new MessageEntity(databaseContext, message);
    }
    
    private static Expression<Func<MessageModel, bool>> GetExpression(string propertyName, object value)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(MessageModel), "message");
        MemberExpression property = Expression.Property(parameter, propertyName);
        ConstantExpression constant = Expression.Constant(value);
        BinaryExpression equals = Expression.Equal(property, constant);

        return Expression.Lambda<Func<MessageModel, bool>>(equals, parameter);
    }
}