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
using Aiel.Emailing;
using OpenIddict.Abstractions;
using System.Security.Claims;
using System.Security.Principal;

namespace Aiel.Users;

public interface ICurrentUser : IPrincipal
{
    [MaybeNull]
    UserId Id { get; }

    Boolean IsAuthenticated { get; }

    String UserName { get; }

    String FirstName { get; }

    String LastName { get; }

    Email? Email { get; }

    Boolean EmailVerified { get; }

    [NotNull]
    String[] Roles { get; }

    Claim? FindClaim(String claimType);

    [return: NotNull]
    Claim[] FindClaims(String claimType) => [];

    [return: NotNull]
    Claim[] GetAllClaims() => [];

    Boolean IsInRole(Role role);
    Boolean IsInRole(RoleId roleId);
}

public class CurrentUser(IPrincipal principal) : ClaimsPrincipal(principal), ICurrentUser
{
    public static readonly CurrentUser Empty = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private UserId? _id;
    private String? _firstName;
    private String? _lastName;
    private String? _email;
    private Boolean? _emailVerified;
    private Boolean? _phoneNumberVerified;
    private NorthAmericanPhoneNumber? _phoneNumber;
    private String? _username;

    public UserId Id => _id ??= UserId.From(FindValue(OpenIddictConstants.Claims.Subject, Guid.Empty));
    public Boolean IsAuthenticated => Identity?.IsAuthenticated ?? false;
    public String UserName => _username ??= FindValue(OpenIddictConstants.Claims.Name, String.Empty);
    public String FirstName => _firstName ??= FindValue(OpenIddictConstants.Claims.GivenName, String.Empty);
    public String LastName => _lastName ??= FindValue(OpenIddictConstants.Claims.FamilyName, String.Empty);
    public Email? Email => _email ??= FindValue(OpenIddictConstants.Claims.Email, Email.Empty);
    public Boolean EmailVerified => _emailVerified ??= FindValue(OpenIddictConstants.Claims.EmailVerified, false);
    public NorthAmericanPhoneNumber? PhoneNumber => _phoneNumber ??= FindValue(OpenIddictConstants.Claims.PhoneNumber, NorthAmericanPhoneNumber.Empty);
    public Boolean PhoneNumberVerified => _phoneNumberVerified ??= FindValue(OpenIddictConstants.Claims.PhoneNumberVerified, false);
    public String[] Roles { get; private set; } = [];

    public Claim? FindClaim(String claimType)
    {
        throw new NotImplementedException();
    }

    public Boolean IsInRole(Role role)
    {
        throw new NotImplementedException();
    }

    public Boolean IsInRole(RoleId roleId)
    {
        throw new NotImplementedException();
    }

    private String FindValue(String claimType, String defaultValue)
    {
        var claim = FindFirst(claimType);
        return claim?.Value ?? defaultValue ?? String.Empty;
    }

    private Boolean FindValue(String claimType, Boolean defaultValue)
    {
        var claim = FindFirst(claimType);
        return Boolean.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    private Guid FindValue(String claimType, Guid defaultValue)
    {
        var claim = FindFirst(claimType);
        return Guid.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    private Email FindValue(String claimType, Email defaultValue)
    {
        var claim = FindFirst(claimType);
        return Email.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    private NorthAmericanPhoneNumber FindValue(String claimType, NorthAmericanPhoneNumber defaultValue)
    {
        var claim = FindFirst(claimType);
        return NorthAmericanPhoneNumber.TryParse(claim?.Value, out var result) ? result : defaultValue;
    }

    public static CurrentUser FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var identity = principal.Identity;
        if (identity is null)
        {
            return Empty;
        }

        if (identity?.IsAuthenticated != true)
        {
            return WellKnownUsers.Anonymous;
        }

        return new CurrentUser(principal);
    }
}

public interface IUserAccessor
{
    ICurrentUser Current { get; }
    IDisposable Change(ICurrentUser? user);
}

public class UserAccessor(AmbientUserContext context) : IUserAccessor
{
    private readonly AmbientUserContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public ICurrentUser Current => _context.Current;

    public IDisposable Change(ICurrentUser? user)
    {
        var previous = _context.Current;
        _context.Current = user ?? CurrentUser.Empty;
        return new UserChangeContext(() => _context.Current = previous);
    }
}

public sealed class AmbientUserContext
{
    private readonly AsyncLocal<ICurrentUser?> _current = new();

    public ICurrentUser Current
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
