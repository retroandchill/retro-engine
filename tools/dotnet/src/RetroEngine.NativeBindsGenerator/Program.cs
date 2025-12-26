// See https://aka.ms/new-console-template for more information

using DotMake.CommandLine;
using RetroEngine.NativeBindsGenerator;

Cli.Run<RootCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });
