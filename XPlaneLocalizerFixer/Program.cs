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
builder.Services.AddSingleton<ICorrectedNavDatWriter, CorrectedNavDatWriter>();

using var host = builder.Build();

// simple workflow test
var processor = host.Services
    .GetRequiredService<ILocalizerCorrectionProcessor>();

var writer = host.Services
    .GetRequiredService<ICorrectedNavDatWriter>();

// process all eligible ILS localizers
var corrections = processor.Process();

Console.WriteLine($"Successfully corrected {corrections.Count} localizers.");

if (corrections.Count == 0)
{
    Console.WriteLine("No corrections were generated..");
    return;
}

// generate a new file
var outputPath = Path.Combine(AppContext.BaseDirectory, "user_nav_generated.dat");

writer.Write(outputPath, corrections);

Console.WriteLine($"Generated file: {outputPath}");