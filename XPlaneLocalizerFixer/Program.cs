using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Services;

var builder = Host.CreateApplicationBuilder(args);

//File access
builder.Services.AddSingleton<IFileReader, FileReader>();
builder.Services.AddSingleton<IFileWriter, FileWriter>();

//Parsers
builder.Services.AddSingleton<IAptDatParser, AptDatParser>();
builder.Services.AddSingleton<IEarthNavDatParser, EarthNavDatParser>();

// Repositories
builder.Services.AddTransient<IAirportRepository, AirportRepository>();
builder.Services.AddTransient<INavDataRepository, NavDataRepository>();

//services
builder.Services.AddSingleton<ILocalizerFixerWorkflow, LocalizerFixerWorkflow>();
builder.Services.AddSingleton<ILocalizerRunwayMatcher, LocalizerRunwayMatcher>();
builder.Services.AddSingleton<ILocalizerNavAidMatcher, LocalizerNavAidMatcher>();
builder.Services.AddSingleton<ILocalizerCorrector, LocalizerCorrector>();
builder.Services.AddSingleton<ILocalizerCorrectionProcessor, LocalizerCorrectionProcessor>();
builder.Services.AddSingleton<ICorrectedNavDatWriter, CorrectedNavDatWriter>();

// Configuration
builder.Services.Configure<FilePathOptions>(
    builder.Configuration.GetSection(FilePathOptions.SectionName));
builder.Services.Configure<LocalizerCorrectionOptions>(
    builder.Configuration.GetSection(LocalizerCorrectionOptions.SectionName));

using var host = builder.Build();

// get the configured file paths.
var fileOptions = host.Services
    .GetRequiredService<IOptions<FilePathOptions>>()
    .Value;

var outputDirectory = Path.GetFullPath(
    fileOptions.OutputDirectory,
    AppContext.BaseDirectory);

var outputPath = Path.Combine(
    outputDirectory,
    fileOptions.OutputFileName);

var workFlow = host.Services.GetRequiredService<ILocalizerFixerWorkflow>();

var result = workFlow.Execute(outputPath);

Console.WriteLine($"Successfully corrected {result.CorrectedCount} localizers.");

if (result.OutputPath is null)
{
    Console.WriteLine("No corrections were generated.");
    return;
}

Console.WriteLine($"Generated file: {outputPath}");