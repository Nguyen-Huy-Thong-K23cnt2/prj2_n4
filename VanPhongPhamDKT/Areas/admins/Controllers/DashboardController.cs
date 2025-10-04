using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("admins")]
[Authorize(Roles = "admin")]   // ✅ chỉ Admin mới vào được
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
