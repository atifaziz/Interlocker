``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.500
  [Host] : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  Core   : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|                    Method |      N |     Mean |     Error |    StdDev | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|-------------------------- |------- |---------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
| InterlockerOptimizedTests | 100000 | 1.534 ms | 0.0244 ms | 0.0228 ms |           - |           - |           - |                24 B |
|          InterlockerTests | 100000 | 9.229 ms | 0.1829 ms | 0.4945 ms |   5593.7500 |           - |           - |          17600024 B |
