using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Test;

var builder = Host.CreateApplicationBuilder(args);

//File access
builder.Services.AddSingleton<IFileReader, FileReader>();

//Parsers
builder.Services.AddSingleton<IAptDatParser, AptDatParser>();

// Repositories
builder.Services.AddTransient<IAirportRepository, AirportRepository>();

builder.Services.AddTransient<FileReaderTest>();
builder.Services.AddTransient<AptDatParserTest>();

using var host = builder.Build();

var fileReadTest = host.Services.GetRequiredService<FileReaderTest>();

fileReadTest.Run("earth_nav.dat");

var aptDatParserTest = host.Services.GetRequiredService<AptDatParserTest>();
aptDatParserTest.Run("VHHH");