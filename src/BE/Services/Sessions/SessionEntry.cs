﻿using System.Security.Claims;

namespace Chats.BE.Services.Sessions;

public record SessionEntry
{
    public required int UserId { get; init; }
    public required string UserName { get; init; }
    public required string Role { get; init; }
    public required string? Provider { get; init; }
    public required string? Sub { get; init; }

    public virtual List<Claim> ToClaims()
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString(), ClaimValueTypes.Integer32),
            new Claim(ClaimTypes.Name, UserName),
            new Claim(ClaimTypes.Role, Role)
        ];

        if (Provider != null)
        {
            claims.Add(new Claim("provider", Provider));
        }

        if (Sub != null)
        {
            claims.Add(new Claim("provider-sub", Sub));
        }
        return claims;
    }

    public static SessionEntry FromClaims(ClaimsPrincipal claims)
    {
        return new SessionEntry
        {
            UserId = int.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)!.Value),
            UserName = claims.FindFirst(ClaimTypes.Name)!.Value,
            Role = claims.FindFirst(ClaimTypes.Role)!.Value,
            Provider = claims.FindFirst("provider")?.Value,
            Sub = claims.FindFirst("provider-sub")?.Value
        };
    }
}
