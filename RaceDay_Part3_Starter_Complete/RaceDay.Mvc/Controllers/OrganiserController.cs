using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services; using System.Net;
namespace RaceDay.Mvc.Controllers;
public class OrganiserController(RaceDayApiClient api):Controller {
 private bool Allowed()=>HttpContext.Session.GetString("Role")=="Organiser";
 public async Task<IActionResult> Index(){if(!Allowed())return RedirectToAction("Login","Account");ViewBag.Name=HttpContext.Session.GetString("Name");ViewBag.Events=await api.Get<List<EventVm>>("api/events")??new();return View();}
 [HttpGet] public IActionResult Create(){if(!Allowed())return RedirectToAction("Login","Account");return View(new EventVm());}
 [HttpPost] public async Task<IActionResult> Create(EventVm vm){if(!Allowed())return RedirectToAction("Login","Account");var r=await api.Send(HttpMethod.Post,"api/events",new{Title=vm.Title,Description=vm.Description,EventDate=vm.EventDate,Location=vm.Location,Distance=vm.Distance,EventType=vm.EventType});if(!r.ok){ViewBag.Error=r.message==""?"Could not save event. Check the API connection.":r.message;return View(vm);}return RedirectToAction("Index");}
 public async Task<IActionResult> Delete(int id){if(!Allowed())return RedirectToAction("Login","Account");await api.Send(HttpMethod.Delete,$"api/events/{id}");return RedirectToAction("Index");}
}
