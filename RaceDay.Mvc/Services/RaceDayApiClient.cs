using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace RaceDay.Mvc.Services;
public class RaceDayApiClient
{
 private readonly HttpClient _http; private readonly IHttpContextAccessor _accessor; private readonly IConfiguration _config;
 private static readonly JsonSerializerOptions Json = new(){PropertyNameCaseInsensitive=true};
 public RaceDayApiClient(HttpClient http,IHttpContextAccessor accessor,IConfiguration config){_http=http;_accessor=accessor;_config=config;_http.BaseAddress=new Uri(config["Api:BaseUrl"]??"http://localhost:5000/");}
 private HttpRequestMessage Request(HttpMethod method,string path,object? body=null){var r=new HttpRequestMessage(method,path); var cookie=_accessor.HttpContext?.Session.GetString("ApiCookie"); if(!string.IsNullOrWhiteSpace(cookie))r.Headers.Add("Cookie",cookie); if(body!=null)r.Content=new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json"); return r;}
 public async Task<(bool ok,string message,JsonElement data)> Send(HttpMethod method,string path,object? body=null){try{using var req=Request(method,path,body); using var res=await _http.SendAsync(req); if(res.Headers.TryGetValues("Set-Cookie",out var cookies)){var c=cookies.FirstOrDefault(x=>x.StartsWith("RaceDay.Session=")); if(c!=null)_accessor.HttpContext?.Session.SetString("ApiCookie",c.Split(';')[0]);} var raw=await res.Content.ReadAsStringAsync(); JsonElement data=default; try{using var doc=JsonDocument.Parse(string.IsNullOrWhiteSpace(raw)?"{}":raw);data=doc.RootElement.Clone();}catch{} string msg=""; if(data.ValueKind==JsonValueKind.Object && data.TryGetProperty("message",out var m))msg=m.GetString()??""; return(res.IsSuccessStatusCode,msg,data);}catch(Exception ex){return(false,"Could not connect to RaceDay API: "+ex.Message,default);}}
 public async Task<T?> Get<T>(string path){var x=await Send(HttpMethod.Get,path);return x.ok?x.data.Deserialize<T>(Json):default;}
}
