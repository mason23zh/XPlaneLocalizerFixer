using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Test;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IFileReader, FileReader>();
builder.Services.AddSingleton<AptDatParser>();

builder.Services.AddTransient<FileReaderTest>();
builder.Services.AddTransient<AptDatParserTest>();
builder.Services.AddTransient<IAirportRepository, AirportRepository>();

using IHost host = builder.Build();

var fileReadTest  = host.Services.GetRequiredService<FileReaderTest>();

fileReadTest.Run("earth_nav.dat");

var aptDatParserTest = host.Services.GetRequiredService<AptDatParserTest>();
aptDatParserTest.Run("VHHH");
