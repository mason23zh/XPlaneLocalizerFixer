using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Services;

var builder = Host.CreateApplicationBuilder(args);

//File access
builder.Services.AddSingleton<IFileReader, FileReader>();

//Parsers
builder.Services.AddSingleton<IAptDatParser, AptDatParser>();
builder.Services.AddSingleton<IEarthNavDatParser, EarthNavDatParser>();

// Repositories
builder.Services.AddTransient<IAirportRepository, AirportRepository>();
builder.Services.AddTransient<INavDataRepository, NavDataRepository>();

//services
builder.Services.AddSingleton<ILocalizerRunwayMatcher, LocalizerRunwayMatcher>();
builder.Services.AddSingleton<ILocalizerNavAidMatcher, LocalizerNavAidMatcher>();
builder.Services.AddSingleton<ILocalizerCorrector, LocalizerCorrector>();
builder.Services.AddSingleton<ILocalizerCorrectionProcessor, LocalizerCorrectionProcessor>();

using var host = builder.Build();