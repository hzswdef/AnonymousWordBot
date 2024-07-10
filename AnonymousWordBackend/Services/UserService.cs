using System.Linq.Expressions;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.Entities;
using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Services;

public class UserService (DatabaseContext databaseContext)
{
    public async Task<UserEntity?> LoadById(long id)
    {
        UserModel? user = await databaseContext
            .Users
            .FirstOrDefaultAsync(user => user.Id == id);

        return user == null
            ? null
            : new UserEntity(databaseContext, user);
    }
    
    public async Task<UserEntity?> LoadByProperties(Dictionary<string, object> properties)
    {
        IQueryable<UserModel> query = databaseContext.Users.AsQueryable();

        foreach (var property in properties)
            query = query.Where(GetExpression(property.Key, property.Value));

        UserModel? user = await query.FirstOrDefaultAsync();

        return user == null
            ? null
            : new UserEntity(databaseContext, user);
    }
    
    private static Expression<Func<UserModel, bool>> GetExpression(string propertyName, object value)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(UserModel), "user");
        MemberExpression property = Expression.Property(parameter, propertyName);
        ConstantExpression constant = Expression.Constant(value);
        BinaryExpression equals = Expression.Equal(property, constant);

        return Expression.Lambda<Func<UserModel, bool>>(equals, parameter);
    }
}