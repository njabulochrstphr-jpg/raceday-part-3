using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Services;using System.Net;
namespace RaceDay.Mvc.Controllers;
public class ProfileController(RaceDayApiClient api):Controller{public async Task<IActionResult> Index(){if(string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))return RedirectToAction("Login","Account");var r=await api.Send(HttpMethod.Get,"api/users/me");ViewBag.Profile=r.data;return View();}}
