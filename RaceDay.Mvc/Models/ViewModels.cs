namespace RaceDay.Mvc.Models;
public class LoginVm { public string Email {get;set;}=""; public string Password {get;set;}=""; }
public class RegisterVm { public string Name {get;set;}=""; public string Email {get;set;}=""; public string Password {get;set;}=""; public string Role {get;set;}="Participant"; }
public class EventVm { public int Id {get;set;} public string Title {get;set;}=""; public string? Description {get;set;} public DateTime EventDate {get;set;}=DateTime.Today.AddDays(7); public string Location {get;set;}=""; public double Distance {get;set;} public string EventType {get;set;}="Run"; public string Status {get;set;}="Scheduled"; public string Organiser {get;set;}=""; }
