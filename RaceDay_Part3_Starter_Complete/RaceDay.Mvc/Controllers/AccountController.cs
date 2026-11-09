using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services; using System.Net; using System.Text.Json;
namespace RaceDay.Mvc.Controllers;
public class AccountController(RaceDayApiClient api):Controller {
 [HttpGet] public IActionResult Login()=>View(new LoginVm());
 [HttpPost] public async Task<IActionResult> Login(LoginVm vm){if(!ModelState.IsValid)return View(vm);var r=await api.Send(HttpMethod.Post,"api/auth/login",vm);if(!r.ok){ViewBag.Error=r.message==""?"Login failed. Check your details and try again.":r.message;return View(vm);}var u=r.data.GetProperty("user");HttpContext.Session.SetString("Name",u.GetProperty("name").GetString()??"");HttpContext.Session.SetString("Role",u.GetProperty("role").GetString()??"");return RedirectToAction("Index",HttpContext.Session.GetString("Role")=="Organiser"?"Organiser":"Participant");}
 [HttpGet] public IActionResult Register()=>View(new RegisterVm());
 [HttpPost] public async Task<IActionResult> Register(RegisterVm vm){var r=await api.Send(HttpMethod.Post,"api/auth/register",vm);if(!r.ok){ViewBag.Error=r.message==""?"Registration failed.":r.message;return View(vm);}TempData["Message"]="Account created. Please log in.";return RedirectToAction("Login");}
 public async Task<IActionResult> Logout(){await api.Send(HttpMethod.Post,"api/auth/logout");HttpContext.Session.Clear();return RedirectToAction("Index","Home");}
}
