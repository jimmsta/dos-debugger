This project aims to implement a usable debugger for 16-bit DOS executables. It attempts to fill the gap of modern debuggers as well as to explore techniques in binary analysis.

# Objective #

The starting point of the project is the observation of the lack of some crucial features in contemporary binary analysis software:

**Lack of 16-bit decompiler**. While disassemlers for 16-bit executables are common, there appear to be no contemporary decompilers for 16-bit executables. This is totally understandable as 32-bit executables are by far the dominant scenario where such software may be useful. It is also natural to predict that 64-bit decompilers will soon appear, following the gradual adoption of 64-bit hardware and software. However, it is unlikely that 16-bit decompilers will be the focus of industrial effort any time soon.

**Lack of dynamic analysis**. Most of today's disassemblers and decompilers analyze binary code statically. While a disassembler can and is usually integrated into a debugger to provide some degree of dynamic analysis, such analysis typically relies solely on the human analyst; automatic analysis is seldom performed. For decompilers, even this level of dynamic analysis is seldom present, probably because of the complexity of implementing it.

**Retirement of 16-bit OS**. With the retirement of 16-bit operating systems, such as MS-DOS, legacy 16-bit applications cannot even run natively in a modern environment, not to mention being debugged natively. A common workaround is to launch the executable in an emulator, in which a 16-bit debugger can also be emulated. This revives old binary analysis software which were designed specifically for 16-bit executables of that time; however those programs miss some features that would be expected in a contemporary environment.

In contrast to the lack of tools for analyzing 16-bit executables, such analysis may still be needed from time to time. This project aims to provide a tool that facilitates such analysis. In addition to its designed use, such a tool may be interesting in its own right in the field of binary analysis research.

# Approach #

In addition to filling the missing features of current software, this project attempts to tackle the problem of binary analysis from a slightly different angle. Some key approaches planned are:

**Debugging in an emulated environment**. This ensures that the debugger has absolutely full knowledge and control of the program's behavior, which is ideal in debugging sophisticated software. Not only can this be used for legacy 16-bit executable where it cannot be run natively, this approach could potentially be used to debug modern applications as well, as long as we have an emulated run-time environment for it (such as WINE for Win32 applications).

**Integrated disassembler/decompiler with analysis**. The debugger should have a modern GUI interface that integrates the disassembler and decompiler. Moreover, the disassembler must not just display the instruction dumbly; it must carry out analysis on the programs behavior and logic to aid a human analyst in understanding the program. If sufficiently advanced, such analysis can even be used to optimize a program from its binary code, not from source!

**Dynamic (possibly simulated) analysis**. With the integration of an advanced debugger and an emulator to execute the program, the disassembler/decompiler could be possible to carry out dynamic analysis of the program by actually running it and tracing its run-time characteristics. For suitable subroutines, it could even generated random inputs to test the outputs, which may be helpful to identify the purpose of the subroutine, or merely to test the robustness of the subroutine.

In summary, the project aims to deliver a concise but advanced tool that demonstrates key ideas and techniques for the problems it aims to solves.

# Design #

Due to the nature of this project being more toward a research project than a real product, there are certain design choices made:

**Permissive license**. The source code will use a permissive license which allows it to be integrated into real-world applications smoothly.

**Limited coverage**. The project will only implement enough code to show the key ideas. This means, therefore, that it will not put much effort to provide rich features, wide compatibility, etc. For example, it will likely only run under Windows, read 16-bit DOS .EXE format, recognize 8086 instructions, and outputs assembly in Intel syntax. However, such limited coverage is already sufficient to implement most interesting techniques, which should be easily extensible to modern usage if desired.

**Modular design**. The source code shall be arranged in _orthogonal_ modules, so that each module of code may be used easily in other applications. The modular design is critical to reduce the number of times we need to reinvent the wheel. To simplify this task, the source code will be written in C++.

# Reference #

Below is a summary of a few popular disassemblers/decompilers known today:

**IDA Pro**. Highly professional, feature rich, and costly. However, currently its hex-rays decompiler does not support 16-bit binaries, and its Bochs debugger plugin does not debug 16-bit binaries. These may get improved in the future, but there may be other priorities (for example, 64-bit support is likely more urgent than 16-bit for them).

**Bochs emulator**. Bundled with a disassembler/debugger. However, it only emulates the PC, not the operating system. While this may be solved by running a DOS clone (e.g. FreeDOS) inside the virtual machine, the emulator is focused on _emulation_ after all, which means it lacks some key features in even assembly level binary analysis. In addition, it provides too much hardware emulation than needed for analyzing a 16-bit program.

**DOSBox emulator**. The key advantage is an built-in emulation of 16-bit DOS environment, with excellent compatibility with DOS programs (especially games).  This environment fits the need of this project very well. The emulator is also bundled with a assembly-level debugger, though it suffers from the same weakness of lacking analysis functions as the Bochs emulator.

**OllyDbg debugger**. Very nice debugger for Windows. However, it doesn't support 16-bit executable.

**LLVM project**. Modern, well-maintained compiler infrastructure. Whether it could be adopted for decompilation is yet to be investigated.

There are a few other decompilers out there, but none of them appear to work, and none of them come with a GUI that provides the most basic functionalities.

# Status #

This project is only just started. The author doesn't have much technical background in binary analysis, so this project may well take him years to accomplish - if he's still interested in the subject by then.