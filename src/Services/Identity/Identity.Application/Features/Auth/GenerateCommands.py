import os

commands = [
    {"name": "Login", "type": "Command", "req": "LoginRequest", "res": "Result<LoginResponse>", "fields": "LoginRequest Request, string IpAddress"},
    {"name": "LoginWith2FA", "type": "Command", "req": "TwoFactorLoginRequest", "res": "Result<LoginResponse>", "fields": "TwoFactorLoginRequest Request, string IpAddress"},
    {"name": "Register", "type": "Command", "req": "RegisterRequest", "res": "Result<UserDto>", "fields": "RegisterRequest Request"},
    {"name": "ConfirmEmail", "type": "Command", "req": "ConfirmEmailRequest", "res": "Result", "fields": "ConfirmEmailRequest Request"},
    {"name": "ResendConfirmation", "type": "Command", "req": "string", "res": "Result", "fields": "string Email"},
    {"name": "ForgotPassword", "type": "Command", "req": "string", "res": "Result", "fields": "string Email"},
    {"name": "ResetPassword", "type": "Command", "req": "ResetPasswordRequest", "res": "Result", "fields": "ResetPasswordRequest Request"},
    {"name": "RefreshToken", "type": "Command", "req": "string", "res": "Result<TokenResponse>", "fields": "string Token, string IpAddress"},
    {"name": "RevokeToken", "type": "Command", "req": "string", "res": "Result", "fields": "string Token, string IpAddress"},
    {"name": "Logout", "type": "Command", "req": "string", "res": "Result", "fields": "string UserId, string Token"},
    {"name": "ChangePassword", "type": "Command", "req": "ChangePasswordRequest", "res": "Result", "fields": "string UserId, ChangePasswordRequest Request"},
    {"name": "LogoutAll", "type": "Command", "req": "string", "res": "Result", "fields": "string UserId"},
    {"name": "ProcessExternalLogin", "type": "Command", "req": "ExternalLoginInfo", "res": "Result<ExternalAuthResult>", "fields": "ExternalLoginInfo Info"},
    {"name": "GenerateAuthCode", "type": "Command", "req": "string", "res": "Result<string>", "fields": "string UserId"},
    {"name": "ExchangeCodeForTokens", "type": "Command", "req": "string", "res": "Result<LoginResponse>", "fields": "string Code, string IpAddress"},
    {"name": "CheckUsernameAvailability", "type": "Query", "req": "string", "res": "bool", "fields": "string Username"},
    {"name": "CheckEmailAvailability", "type": "Query", "req": "string", "res": "bool", "fields": "string Email"},
    {"name": "GetCurrentUser", "type": "Query", "req": "string", "res": "Result<UserDto>", "fields": "string UserId"}
]

base_dir = "/Users/macbook/Desktop/auction-platform-microservices/src/Services/Identity/Identity.Application/Features/Auth"

for cmd in commands:
    name = cmd["name"]
    ctype = cmd["type"]
    res = cmd["res"]
    fields = cmd["fields"]
    
    dir_path = os.path.join(base_dir, f"{ctype}s", name)
    os.makedirs(dir_path, exist_ok=True)
    
    file_path = os.path.join(dir_path, f"{name}{ctype}.cs")
    
    interface = f"I{ctype}<{res.replace('Result<', '').replace('>', '')}>" if res.startswith("Result<") else (f"I{ctype}" if res == "Result" else f"I{ctype}<{res}>")
    handler_interface = f"I{ctype}Handler<{name}{ctype}, {res.replace('Result<', '').replace('>', '')}>" if res.startswith("Result<") else (f"I{ctype}Handler<{name}{ctype}>" if res == "Result" else f"I{ctype}Handler<{name}{ctype}, {res}>")
    
    content = f"""namespace Identity.Application.Features.Auth.{ctype}s.{name};

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record {name}{ctype}({fields}) : {interface};

public class {name}{ctype}Handler(
    IUserService userService,
    SignInManager<ApplicationUser> signInManager,
    ITokenGenerationService tokenService,
    IAuthHelper authHelper,
    IConfiguration configuration,
    ILogger<{name}{ctype}Handler> logger) : {handler_interface}
{{
    public async Task<{res}> Handle({name}{ctype} request, CancellationToken cancellationToken)
    {{
        throw new NotImplementedException();
    }}
}}
"""
    with open(file_path, "w") as f:
        f.write(content)
        
print("Generated commands and queries")
