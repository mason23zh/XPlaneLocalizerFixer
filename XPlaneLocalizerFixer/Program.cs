using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Services;
using XPlaneLocalizerFixer.Test;

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

builder.Services.AddTransient<FileReaderTest>();
builder.Services.AddTransient<AptDatParserTest>();
builder.Services.AddTransient<EarthNavDatParserTest>();
builder.Services.AddTransient<LocalizerRunwayMatcherTest>();

using var host = builder.Build();

var fileReadTest = host.Services.GetRequiredService<FileReaderTest>();

fileReadTest.Run("earth_nav.dat");

var aptDatParserTest = host.Services.GetRequiredService<AptDatParserTest>();
aptDatParserTest.Run("VHHH");

var earthNavDatParserTest = host.Services.GetRequiredService<EarthNavDatParserTest>();
earthNavDatParserTest.Run("SBGL");

var matcher = host.Services.GetRequiredService<LocalizerRunwayMatcherTest>();
matcher.Run("CYWG", "31");