# SharpMonoInjector
SharpMonoInjector is a tool for injecting assemblies into Mono embedded applications, commonly Unity Engine based games. The target process does not have to be restarted in order to inject an updated version of the assembly. Instead, you need to properly eject the assembly which this tool simplifies, and then inject again.

Support for x64 processes has now been added.

In order for the injector to work, the load/unload methods need to match the following method signature:

```sh
static void Method()
```
# Note
Processes running the newer mono runtime (mono-2.0-bdwgc.dll) are currently not injectable with this tool. The process crashes when executing mono_thread_attach(), and that's about all I know at the moment. I will fix this whenever I have time for it.
