using System.Linq.Expressions;
using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.Entities;
using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Services;

public class BanListService (DatabaseContext databaseContext)
{
    public async Task<BanListEntity?> LoadById(long id)
    {
        BanListModel? banList = await databaseContext
            .BanList
            .FirstOrDefaultAsync(banList => banList.Id == id);

        return banList == null
            ? null
            : new BanListEntity(databaseContext, banList);
    }
    
    public async Task<BanListEntity?> LoadByProperties(Dictionary<string, object> properties)
    {
        IQueryable<BanListModel> query = databaseContext.BanList.AsQueryable();

        foreach (var property in properties)
            query = query.Where(GetExpression(property.Key, property.Value));

        BanListModel? banList = await query.FirstOrDefaultAsync();

        return banList == null
            ? null
            : new BanListEntity(databaseContext, banList);
    }
    
    private static Expression<Func<BanListModel, bool>> GetExpression(string propertyName, object value)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(BanListModel), "banList");
        MemberExpression property = Expression.Property(parameter, propertyName);
        ConstantExpression constant = Expression.Constant(value);
        BinaryExpression equals = Expression.Equal(property, constant);

        return Expression.Lambda<Func<BanListModel, bool>>(equals, parameter);
    }
}