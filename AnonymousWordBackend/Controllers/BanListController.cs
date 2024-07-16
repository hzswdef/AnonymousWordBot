using AnonymousWordBackend.Contexts;
using AnonymousWordBackend.ControllerParams;
using AnonymousWordBackend.Entities;
using AnonymousWordBackend.Models;
using AnonymousWordBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnonymousWordBackend.Controllers;

[ApiController]
[Route("/api/ban_list")]
public class BanListController : ControllerBase
{
    private readonly ILogger<BanListController> _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly BanListService _banListService;
    private readonly UserService _userService;

    public BanListController(
        ILogger<BanListController> logger,
        DatabaseContext databaseContext,
        BanListService banListService,
        UserService userService)
    {
        _logger = logger;
        _databaseContext = databaseContext;
        _banListService = banListService;
        _userService = userService;
    }

    /// <summary>
    /// Ban user to disallow messaging the issuer.
    /// </summary>
    /// <param name="data">Params.</param>
    /// <returns>BanList model.</returns>
    [HttpPost("ban")]
    public async Task<IActionResult> Ban([FromBody] PostBanList data)
    {
        UserEntity? issuer = await _userService.LoadByProperties(new Dictionary<string, object> {
        {
            "TelegramId", data.IssuerId
        } });
        
        if (issuer == null)
            return NotFound("Issuer UserModel not found");
        
        UserEntity? banned = await _userService.LoadByProperties(new Dictionary<string, object> {
        {
            "TelegramId", data.BannedId
        } });
        
        if (banned == null)
            return NotFound("Banned UserModel not found.");

        Dictionary<string, object>? properties = JObject
            .FromObject(data, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore })
            .ToObject<Dictionary<string, object>>();
        if (properties == null)
            return NotFound("Missing BanList properties.");
        
        BanListEntity? existingBanList = await _banListService.LoadByProperties(properties);

        if (existingBanList != null)
        {
            try
            {
                existingBanList = await existingBanList.SetBanned();
                return Ok(existingBanList);
            }
            catch (BanListUserIsAlreadyUnbanned e)
            {
                return Conflict(e);
            }
        } 

        BanListModel banList = new()
        {
            MessageId = data.MessageId,
            IssuerId = data.IssuerId,
            BannedId = data.BannedId,
            BannedOn = DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        _databaseContext.BanList.Add(banList);
        await _databaseContext.SaveChangesAsync();
        
        return Ok(banList);
    }
    
    /// <summary>
    /// Unban user to allow messaging the issuer.
    /// </summary>
    /// <param name="data">Params.</param>
    /// <returns>BanList model.</returns>
    [HttpPost("unban")]
    public async Task<IActionResult> Unban([FromBody] PostBanList data)
    {
        Dictionary<string, object>? properties = JObject
            .FromObject(data, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore })
            .ToObject<Dictionary<string, object>>();
        if (properties == null)
            return NotFound("Missing BanList properties.");
        
        BanListEntity? existingBanList = await _banListService.LoadByProperties(properties);

        if (existingBanList == null)
            return NotFound("User isn't banned.");
        try
        {
            existingBanList = await existingBanList.SetUnbanned();
            return Ok(existingBanList);
        }
        catch (BanListUserIsAlreadyUnbanned e)
        {
            return Conflict(e);
        }
    }
}