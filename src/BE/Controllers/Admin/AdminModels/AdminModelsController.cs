﻿using Chats.BE.Controllers.Admin.AdminModels.Dtos;
using Chats.BE.Controllers.Admin.Common;
using Chats.BE.Controllers.Common;
using Chats.BE.DB;
using Chats.BE.DB.Jsons;
using Chats.BE.Infrastructure;
using Chats.BE.Services;
using Chats.BE.Services.Conversations;
using Chats.BE.Services.Conversations.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

namespace Chats.BE.Controllers.Admin.AdminModels;

[Route("api/admin"), AuthorizeAdmin]
public class AdminModelsController(ChatsDB db) : ControllerBase
{
    [HttpGet("models")]
    public async Task<ActionResult<AdminModelDto[]>> GetAdminModels(bool all, CancellationToken cancellationToken)
    {
        IQueryable<ChatModel> query = db.ChatModels;
        if (!all) query = query.Where(x => x.Enabled);

        return await query
            .OrderBy(x => x.Rank)
            .Select(x => new AdminModelDtoTemp
            {
                Enabled = x.Enabled,
                FileConfig = x.FileConfig,
                FileServiceId = x.FileServiceId,
                ModelConfig = x.ModelConfig,
                ModelId = x.Id,
                ModelKeysId = x.ModelKeysId,
                ModelProvider = x.ModelProvider,
                ModelVersion = x.ModelVersion,
                Name = x.Name,
                PriceConfig = x.PriceConfig,
                Rank = x.Rank,
                Remarks = x.Remarks,
            })
            .AsAsyncEnumerable()
            .Select(x => x.ToDto())
            .ToArrayAsync(cancellationToken);
    }

    [HttpPut("models/{modelId:guid}")]
    public async Task<ActionResult> UpdateModel(Guid modelId, [FromBody] UpdateModelRequest req, CancellationToken cancellationToken)
    {
        ChatModel? cm = await db.ChatModels.FindAsync([modelId], cancellationToken);
        if (cm == null) return NotFound();

        if (cm.ModelVersion != req.ModelVersion)
        {
            string? modelProvider = await db.ModelKeys
                .Where(x => x.Id == req.ModelKeysId)
                .Select(x => x.Type)
                .SingleOrDefaultAsync(cancellationToken);
                if (modelProvider == null)
                {
                    return this.BadRequestMessage("Model version not found");
                }
            if (modelProvider == null)
            {
                return this.BadRequestMessage("Model version not found");
            }

            cm.ModelProvider = modelProvider;
        }
        req.ApplyTo(cm);
        if (db.ChangeTracker.HasChanges())
        {
            cm.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    [HttpPost("models")]
    public async Task<ActionResult> CreateModel([FromBody] UpdateModelRequest req, CancellationToken cancellationToken)
    {
        string? modelProvider = await db.ModelKeys
            .Where(x => x.Id == req.ModelKeysId)
            .Select(x => x.Type)
            .SingleOrDefaultAsync(cancellationToken);
        if (modelProvider == null)
        {
            return this.BadRequestMessage("Model version not found");
        }

        ChatModel toCreate = new()
        {
            Id = Guid.NewGuid(), 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow, 
            ModelProvider = modelProvider
        };
        req.ApplyTo(toCreate);
        db.ChatModels.Add(toCreate);
        await db.SaveChangesAsync(cancellationToken);

        return Created();
    }

    [HttpPost("models/validate")]
    public async Task<ActionResult> ValidateModel(
        [FromBody] UpdateModelRequest req, 
        [FromServices] ConversationFactory conversationFactory,
        [FromServices] CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        ModelKey? modelProvider = await db.ModelKeys
            .Where(x => x.Id == req.ModelKeysId)
            .SingleOrDefaultAsync(cancellationToken);
        if (modelProvider == null)
        {
            return this.BadRequestMessage("Model version not found");
        }

        ConversationService s = conversationFactory.CreateConversationService(Enum.Parse<KnownModelProvider>(modelProvider.Type), modelProvider.Configs, req.ModelConfig, req.ModelVersion);
        try
        {
            await foreach (ConversationSegment seg in s.ChatStreamed([new UserChatMessage("1+1=?")], new JsonUserModelConfig { }, currentUser, cancellationToken))
            {
            }
            return Ok();
        }
        catch (Exception e)
        {
            return this.BadRequestMessage(e.Message);
        }
    }

    [HttpPut("user-models")]
    public async Task<ActionResult> UpdateUserModels([FromBody] UpdateUserModelRequest req, CancellationToken cancellationToken)
    {
        UserModel? userModel = await db.UserModels
            .FindAsync([req.UserModelId], cancellationToken);
        if (userModel == null) return NotFound();

        userModel.UpdatedAt = DateTime.UtcNow;
        userModel.Models = JSON.Serialize(req.Models.Where(x => x.Enabled));
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
