using System.Security.Claims;
using AutoMapper;
using BCrypt.Net;
using PoultryDistributionSystem.Application.DTOs.Auth;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Enums;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Application.Interfaces.IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public AuthService(IUnitOfWork unitOfWork, Application.Interfaces.IJwtTokenService jwtTokenService, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Find user by username or email
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => (u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail) && !u.IsDeleted,
            cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiry();

        // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshTokenExpiry;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load user profile for full name
        var userProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == user.Id, cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30), // Match JWT expiry
            User = _mapper.Map<UserDto>(user)
        };
    }

    public async Task<UserDto> RegisterAsync(RegisterRequestDto request, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        // Check if username or email already exists
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Username == request.Username || u.Email == request.Email,
            cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Username or email already exists");
        }

        // Parse role
        if (!Enum.TryParse<Domain.Enums.UserRole>(request.Role, out var role))
        {
            throw new ArgumentException("Invalid role");
        }

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            IsActive = true,
            CreatedBy = createdBy
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create user profile
        var userProfile = new UserProfile
        {
            UserId = user.Id,
            FullName = request.FullName,
            Phone = request.Phone,
            CreatedBy = createdBy
        };

        await _unitOfWork.UserProfiles.AddAsync(userProfile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token claims");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Generate new tokens
        var claims = principal.Claims.ToList();
        var newAccessToken = _jwtTokenService.GenerateAccessToken(claims);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenExpiry = _jwtTokenService.GetRefreshTokenExpiry();

        // Update refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = newRefreshTokenExpiry;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Get or create user profile
        var userProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
            up => up.UserId == userId, cancellationToken);

        if (userProfile == null)
        {
            userProfile = new UserProfile
            {
                UserId = user.Id,
                CreatedBy = userId
            };
            await _unitOfWork.UserProfiles.AddAsync(userProfile, cancellationToken);
        }

        // Update profile fields
        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            userProfile.FullName = dto.FullName;
        }
        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            userProfile.Phone = dto.Phone;
        }

        userProfile.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.UserProfiles.UpdateAsync(userProfile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
