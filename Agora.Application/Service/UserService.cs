using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Application.Utils;
using Agora.Auth;
using Agora.Domain.Entities;
using Agora.Domain.Events;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Agora.Application.Service;

public class UserService : IUserService
{
    private readonly AgoraDbContext _db;
    private readonly ITokenService _tokenService;

    public UserService(AgoraDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    // PSEUDOCODE / PLAN (detailed):
    // 1. Validate incoming PagedRequest (assume caller provides reasonable values).
    // 2. Build an IQueryable<User> from _db.Users.
    // 3. If req.Search is provided (non-null/empty), apply a Where filter that searches Name and Email.
    // 4. Execute CountAsync() to get total matching items.
    // 5. Apply Skip/Take for pagination and fetch the page via ToListAsync().
    // 6. Iterate over the fetched items and set the Password property to null for security before returning.
    // 7. Wrap the DB access in a try/catch to provide a clearer error message if something goes wrong.
    //    - Catch Exception and wrap it in an InvalidOperationException with the original exception as InnerException.
    //    - This gives callers context while preserving the original exception chain.
    // 8. Return a PagedResult<User> containing Total and Items.
    //
    // IMPLEMENTATION NOTES:
    // - Use `foreach (var item in items)` to iterate the list (fixes previous syntax errors).
    // - Use the correct property name `Password` (fixes misspelling).
    // - Keep behavior consistent with other methods by providing a clear wrapped exception message.

    public async Task<PagedResult<User>> GetPaged(PagedRequest req)
    {
        try
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(x =>
                    (x.Name != null && x.Name.Contains(req.Search)) ||
                    (x.Email != null && x.Email.Contains(req.Search)));

            var total = await query.CountAsync();

            var items = await query
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            // Null out sensitive fields before returning
            foreach (var item in items)
            {
                item.Password = null;
            }

            return new PagedResult<User> { Total = total, Items = items };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving paged users from the database.", ex);
        }
    }

    // PSEUDOCODE / PLAN:
    // 1. Wrap DB access in try/catch to provide clear error context.
    // 2. Query the Users DbSet for the requested id asynchronously.
    // 3. If user found, set Password = null to avoid returning sensitive data.
    // 4. Return the user (or null if not found).
    // 5. On exception, throw InvalidOperationException with inner exception.

    public async Task<User?> GetById(int id)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user != null)
                user.Password = null;
            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving the user from the database.", ex);
        }
    }

    public async Task<User> Create(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.Password))
            throw new ArgumentException("Password is required", nameof(user.Password));

        // Uniqueness checks
        if (!string.IsNullOrWhiteSpace(user.Email) && await _db.Users.AnyAsync(x => x.Email == user.Email))
            throw new ArgumentException("Email already exists", nameof(user.Email));

        if (!string.IsNullOrWhiteSpace(user.Username) && await _db.Users.AnyAsync(x => x.Username == user.Username))
            throw new ArgumentException("Username already exists", nameof(user.Username));

        if (!string.IsNullOrWhiteSpace(user.Phone) && await _db.Users.AnyAsync(x => x.Phone == user.Phone))
            throw new ArgumentException("Phone number already exists", nameof(user.Phone));

        if (!string.IsNullOrWhiteSpace(user.TaxCode) && await _db.Users.AnyAsync(x => x.TaxCode == user.TaxCode))
            throw new ArgumentException("Tax code already exists", nameof(user.TaxCode));

        if (user.Role == null)
            user.Role = 0; // Default role

        user.Password = PasswordHasher.Hash(user.Password);
        user.CreatedAt = DateTime.UtcNow;

        _db.Users.Add(user);

        // Create Outbox Message
        if (!string.IsNullOrEmpty(user.Email))
        {
            var userRegisteredEvent = new UserRegisteredEvent(user.Id.ToString(), user.Email, user.Name ?? "User");
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow,
                Type = userRegisteredEvent.GetType().AssemblyQualifiedName!,
                Content = JsonConvert.SerializeObject(userRegisteredEvent)
            };
            _db.OutboxMessages.Add(outboxMessage);
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            // Wrap EF-specific exception to provide clearer context for callers
            throw new InvalidOperationException("An error occurred while saving the new user to the database.", dbEx);
        }
        catch (Exception)
        {
            // Preserve stack trace; let higher-level handlers decide what to do
            throw;
        }

        return user;
    }


    // PSEUDOCODE / PLAN:
    // 1. Validate input (user != null).
    // 2. Load existing user by id. If not found -> throw.
    // 3. For each unique field (Email, Username, Phone, TaxCode):
    //    - If incoming value is not null/empty AND different from existing value:
    //      - Query DB for any other user with that value (x.Id != user.Id).
    //      - If exists -> throw argument exception with clear message.
    // 4. Update scalar fields on existing entity (Name, Email, Phone, Username, TaxCode, Address, ImageId, Role).
    //    - If incoming Role is null, keep existing or set default 0.
    // 5. Handle password:
    //    - If incoming Password is not null/whitespace -> hash and set existing.Password.
    //      (Do not require password on update. Always hash provided password.)
    // 6. Save changes inside try/catch:
    //    - Catch DbUpdateException and wrap with InvalidOperationException for clearer context.
    //    - Let other exceptions bubble up to preserve stack trace.
    // 7. Return/complete task.
    //
    // NOTES:
    // - Uniqueness checks exclude the current user (x.Id != user.Id).
    // - Only perform uniqueness checks when the value is provided and different from existing to avoid false positives.
    // - Keep behavior consistent with Create's error wrapping for DB update errors.

    public async Task Update(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var existing = await _db.Users.FindAsync(user.Id);

        if (existing == null)
            throw new ArgumentException("Cannot find user", nameof(user.Id));

        // Normalize inputs (optional trimming)
        string? newEmail = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim();
        string? newUsername = string.IsNullOrWhiteSpace(user.Username) ? null : user.Username.Trim();
        string? newPhone = string.IsNullOrWhiteSpace(user.Phone) ? null : user.Phone.Trim();
        string? newTaxCode = string.IsNullOrWhiteSpace(user.TaxCode) ? null : user.TaxCode.Trim();

        // Uniqueness checks only when changed and provided (exclude current user)
        if (!string.IsNullOrEmpty(newEmail) && !string.Equals(newEmail, existing.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Users.AnyAsync(x => x.Id != existing.Id && x.Email == newEmail))
                throw new ArgumentException("Email already exists", nameof(user.Email));
        }

        if (!string.IsNullOrEmpty(newUsername) && !string.Equals(newUsername, existing.Username, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Users.AnyAsync(x => x.Id != existing.Id && x.Username == newUsername))
                throw new ArgumentException("Username already exists", nameof(user.Username));
        }

        if (!string.IsNullOrEmpty(newPhone) && !string.Equals(newPhone, existing.Phone, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Users.AnyAsync(x => x.Id != existing.Id && x.Phone == newPhone))
                throw new ArgumentException("Phone number already exists", nameof(user.Phone));
        }

        if (!string.IsNullOrEmpty(newTaxCode) && !string.Equals(newTaxCode, existing.TaxCode, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Users.AnyAsync(x => x.Id != existing.Id && x.TaxCode == newTaxCode))
                throw new ArgumentException("Tax code already exists", nameof(user.TaxCode));
        }

        // Update fields
        existing.Name = user.Name?.Trim();
        existing.Email = newEmail;
        existing.Username = newUsername;
        existing.Phone = newPhone;
        existing.TaxCode = newTaxCode;
        existing.Address = user.Address?.Trim();
        existing.ImageId = user.ImageId;

        // Role: if incoming value is null, preserve existing; if still null, default to 0
        if (user.Role.HasValue)
            existing.Role = user.Role;
        else if (!existing.Role.HasValue)
            existing.Role = 0;

        // If password provided -> hash and set. Do NOT require password on update.
        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            existing.Password = PasswordHasher.Hash(user.Password);
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException("An error occurred while updating the user in the database.", dbEx);
        }
        catch (Exception)
        {
            // Preserve stack trace; let higher-level handlers decide what to do
            throw;
        }
    }

    public async Task<User?> UpdateSelf(int userId, UserUpdateRequest req)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(req.Name)) user.Name = req.Name;
        if (!string.IsNullOrEmpty(req.Phone)) user.Phone = req.Phone;
        if (!string.IsNullOrEmpty(req.Email)) user.Email = req.Email;
        if (!string.IsNullOrEmpty(req.Address)) user.Address = req.Address;
        if (!string.IsNullOrEmpty(req.TaxCode)) user.TaxCode = req.TaxCode;

        await _db.SaveChangesAsync();
        return user;
    }

    // 3. Remove the found entity from the DbSet via _db.Users.Remove(user).
    // 4. Wrap SaveChangesAsync() in try/catch:
    //    - Catch DbUpdateException and wrap it in InvalidOperationException with a clear message.
    //    - Catch general Exception and rethrow to preserve stack trace (consistent with other methods).
    // 5. Await SaveChangesAsync() inside the try block.
    public async Task Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return;

        _db.Users.Remove(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            // Wrap EF-specific exception to provide clearer context for callers
            throw new InvalidOperationException("An error occurred while deleting the user from the database.", dbEx);
        }
        catch (Exception)
        {
            // Preserve stack trace; let higher-level handlers decide what to do
            throw;
        }
    }

    //TODO: ĐĂNG NHẬP
    public async Task<LoginResponse> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == req.Email);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (user.Password != PasswordHasher.Hash(req.Password))
        {
            throw new Exception("Invalid password");
        }

        if(user.Role == -1)
        {
            throw new Exception("User is banned");
        }

        var token = _tokenService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            Name = user.Name ?? "",
            Email = user.Email ?? "",
            Role = user.Role.ToString() ?? "0",
        };
    }


    public async Task UpdateRole(int userId, int newRoleId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (newRoleId < -1 || newRoleId > 2)
        {
            throw new ArgumentException($"Invalid role ID: {newRoleId}. Role must be between -1 and 2."); // Assuming roles are -1 (banned), 0 (user), 1 (admin), 2 (staff)
        }

        user.Role = newRoleId;
        await _db.SaveChangesAsync();
    }
}
