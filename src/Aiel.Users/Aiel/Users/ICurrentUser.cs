// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Aiel.Authorization;
using Aiel.Domain.Contacts;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Aiel.Users;

public abstract class CurrentUser
{
    public static readonly CurrentUser Empty = new EmptyCurrentUser();

    public abstract UserId Id { get; }

    public abstract Boolean IsAuthenticated { get; }

    public abstract String UserName { get; }

    public abstract String FirstName { get; }

    public abstract String LastName { get; }

    public abstract Email? Email { get; }

    public abstract Boolean EmailVerified { get; }

    public abstract PhoneNumber? PhoneNumber { get; }
    public abstract Boolean PhoneNumberVerified { get; }
    public abstract String[] Roles { get; }

    public abstract Claim? FindClaim(String claimType);

    [return: NotNull]
    public abstract Claim[] FindClaims(String claimType);

    [return: NotNull]
    public abstract Claim[] GetAllClaims();

    public abstract Boolean IsInRole(String role);
    public abstract Boolean IsInRole(Role role);
    public abstract Boolean IsInRole(RoleId roleId);

    private class EmptyCurrentUser : CurrentUser
    {
        public override UserId Id => default;
        public override Boolean IsAuthenticated => false;
        public override String UserName => String.Empty;
        public override String FirstName => String.Empty;
        public override String LastName => String.Empty;
        public override Email? Email => Email.Empty;
        public override Boolean EmailVerified => false;
        public override PhoneNumber? PhoneNumber => PhoneNumber.Empty;
        public override Boolean PhoneNumberVerified => false;
        public override String[] Roles { get; } = [];
        public override Claim? FindClaim(String claimType) => null;
        [return: NotNull]
        public override Claim[] FindClaims(String claimType) => [];
        [return: NotNull]
        public override Claim[] GetAllClaims() => [];
        public override Boolean IsInRole(String role) => false;
        public override Boolean IsInRole(Role role) => false;
        public override Boolean IsInRole(RoleId roleId) => false;
    }
}

public class PrincipalCurrentUser(ClaimsPrincipal principal) : CurrentUser
{
    private readonly ClaimsPrincipal _principal = principal;
    private UserId? _id;
    private String? _firstName;
    private String? _lastName;
    private String? _email;
    private Boolean? _emailVerified;
    private Boolean? _phoneNumberVerified;
    private PhoneNumber? _phoneNumber;
    private String? _username;

    public override UserId Id => _id ??= UserId.From(FindValue(OpenIddictConstants.Claims.Subject, Guid.Empty));
    public override Boolean IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;
    public override String UserName => _username ??= FindValue(OpenIddictConstants.Claims.Name, String.Empty);
    public override String FirstName => _firstName ??= FindValue(OpenIddictConstants.Claims.GivenName, String.Empty);
    public override String LastName => _lastName ??= FindValue(OpenIddictConstants.Claims.FamilyName, String.Empty);
    public override Email? Email => _email ??= FindValue(OpenIddictConstants.Claims.Email, Email.Empty);
    public override Boolean EmailVerified => _emailVerified ??= FindValue(OpenIddictConstants.Claims.EmailVerified, false);
    public override PhoneNumber? PhoneNumber => _phoneNumber ??= FindValue(OpenIddictConstants.Claims.PhoneNumber, PhoneNumber.Empty);
    public override Boolean PhoneNumberVerified => _phoneNumberVerified ??= FindValue(OpenIddictConstants.Claims.PhoneNumberVerified, false);
    public override String[] Roles { get; } = [];

    private String FindValue(String claimType, String defaultValue)
    {
        var claim = _principal.FindFirst(claimType);
        return claim?.Value ?? defaultValue ?? String.Empty;
    }

    private Boolean FindValue(String claimType, Boolean defaultValue)
    {
        var claim = _principal.FindFirst(claimType);
        return Boolean.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    private Guid FindValue(String claimType, Guid defaultValue)
    {
        var claim = _principal.FindFirst(claimType);
        return Guid.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    private Email FindValue(String claimType, Email defaultValue)
    {
        var claim = _principal.FindFirst(claimType);
        return Email.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    private PhoneNumber FindValue(String claimType, PhoneNumber defaultValue)
    {
        var claim = _principal.FindFirst(claimType);
        return PhoneNumber.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    public static CurrentUser FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var identity = principal.Identity;
        if (identity is null)
        {
            return CurrentUser.Empty;
        }

        if (identity?.IsAuthenticated != true)
        {
            return WellKnownUsers.Anonymous;
        }

        return new PrincipalCurrentUser(principal);
    }

    public override Claim? FindClaim(String claimType)
    {
        throw new NotImplementedException();
    }

    [return: NotNull]
    public override Claim[] FindClaims(String claimType)
    {
        throw new NotImplementedException();
    }

    [return: NotNull]
    public override Claim[] GetAllClaims()
    {
        throw new NotImplementedException();
    }

    public override Boolean IsInRole(String role)
    {
        throw new NotImplementedException();
    }

    public override Boolean IsInRole(Role role)
    {
        throw new NotImplementedException();
    }

    public override Boolean IsInRole(RoleId roleId)
    {
        throw new NotImplementedException();
    }
}

public interface IUserAccessor
{
    CurrentUser Current { get; }
    IDisposable Change(CurrentUser? user);
}

public class UserAccessor(AmbientUserContext context) : IUserAccessor
{
    private readonly AmbientUserContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public CurrentUser Current => _context.Current;

    public IDisposable Change(CurrentUser? user)
    {
        var previous = _context.Current;
        _context.Current = user ?? CurrentUser.Empty;
        return new UserChangeContext(() => _context.Current = previous);
    }
}

public sealed class AmbientUserContext
{
    private readonly AsyncLocal<CurrentUser?> _current = new();

    public CurrentUser Current
    {
        get => _current.Value ?? CurrentUser.Empty;
        internal set => _current.Value = value;
    }
}

internal sealed class UserChangeContext(Action restore) : IDisposable
{
    private Action? _restore = restore ?? throw new ArgumentNullException(nameof(restore));

    public void Dispose()
    {
        var restore = Interlocked.Exchange(ref _restore, null);
        restore?.Invoke();
    }
}
