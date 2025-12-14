using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TriviaWhip.Client;
using TriviaWhip.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<GameEngine>();
builder.Services.AddScoped<AchievementService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<TutorialService>();

await builder.Build().RunAsync();
