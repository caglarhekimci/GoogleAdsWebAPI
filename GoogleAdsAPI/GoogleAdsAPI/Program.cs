global using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using GoogleAdsAPI.ServicesAPI;
using GoogleAdsAPI.ServicesAPI.AccountService;
using GoogleAdsAPI.ServicesAPI.AdGroupService;
using GoogleAdsAPI.ServicesAPI.AdService;
using GoogleAdsAPI.ServicesAPI.CampaignService;
using GoogleAdsAPI.ServicesAPI.KeywordService;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IGoogleAdsService,GoogleAdsService>();
builder.Services.AddSingleton<IKeywordService,KeywordService>();
builder.Services.AddSingleton<IAccountService,AccountService>();
builder.Services.AddSingleton<IAdService,AdService>();
builder.Services.AddSingleton<IAdGroupService, AdGroupService>();
builder.Services.AddSingleton<ICampaignService, CampaignService>();

builder.Services.AddSingleton<GoogleAdsClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
