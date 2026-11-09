using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController(RaceDayApiClient api):Controller {
 private bool Allowed()=>HttpContext.Session.GetString("Role")=="Participant";
 public async Task<IActionResult> Index(){if(!Allowed())return RedirectToAction("Login","Account");ViewBag.Events=await api.Get<List<JsonEvent>>("api/events")??new();return View();}
 public async Task<IActionResult> MyEnrolments(){if(!Allowed())return RedirectToAction("Login","Account");ViewBag.Items=await api.Get<List<JsonEnrolment>>("api/users/me/enrolments")??new();return View();}
 public async Task<IActionResult> MyResults(){if(!Allowed())return RedirectToAction("Login","Account");ViewBag.Items=await api.Get<List<JsonResult>>("api/users/me/results")??new();return View();}
}
public class JsonEvent {public int Id{get;set;} public string Title{get;set;}="";public string? Description{get;set;}public DateTime EventDate{get;set;}public string Location{get;set;}="";public double Distance{get;set;}public string EventType{get;set;}="";public string Status{get;set;}="";}
public class JsonEnrolment {public int Id{get;set;}public string EventTitle{get;set;}="";public string Category{get;set;}="";public DateTime EnrolmentDate{get;set;}public string Status{get;set;}="";}
public class JsonResult {public int Id{get;set;}public string EventTitle{get;set;}="";public string Category{get;set;}="";public string FinishTime{get;set;}="";public int Position{get;set;}}
