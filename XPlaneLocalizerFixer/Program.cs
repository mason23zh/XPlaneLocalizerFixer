using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.ConsoleUI;
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

// Console UI
builder.Services.AddSingleton<ConsoleApplication>();

// Configuration
builder.Services.Configure<FilePathOptions>(
    builder.Configuration.GetSection(FilePathOptions.SectionName));
builder.Services.Configure<LocalizerCorrectionOptions>(
    builder.Configuration.GetSection(LocalizerCorrectionOptions.SectionName));

using var host = builder.Build();

var application = host.Services
    .GetRequiredService<ConsoleApplication>();

application.Run();