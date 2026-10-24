using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Test;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IFileReader, FileReader>();

builder.Services.AddTransient<FileReaderTest>();

using IHost host = builder.Build();

var test  = host.Services.GetRequiredService<FileReaderTest>();

test.Run("earth_nav.dat");