// @file $Program.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// See https://aka.ms/new-console-template for more information

using DotMake.CommandLine;
using RetroEngine.NativeBindsGenerator;

Cli.Run<RootCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });
