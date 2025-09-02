using PinterestClone.Data;

public class UserInfoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public UserInfoService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public (bool IsAdmin, string? UserName, string ProfileImagePath) GetUserInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.Session.GetInt32("UserId");

        if (!userId.HasValue)
            return (false, null, "/images/PP.jpg");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
        return (
            user?.IsAdmin ?? false,
            user?.Name,
            user?.ProfileImagePath ?? "/images/PP.jpg"
        );
    }
}
