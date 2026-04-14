using BenchmarkDotNet.Running;
using MagicArchive.Benchmark;

var switcher = new BenchmarkSwitcher([typeof(ArrayBenchmark), typeof(ListBenchmark)]);
switcher.Run(args);
