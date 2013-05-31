Module Module1

    Function DoOpenExeFile(fileName As String) As Disassembler.Executable

        Dim executable As New Disassembler.Executable(fileName)
        Dim dasm As New Disassembler.ExecutableDisassembler(executable)
        dasm.Analyze()
        Return executable

    End Function

    Function DoOpenLibFile(fileName As String) As Disassembler.ObjectLibrary

        Dim library As Disassembler.ObjectLibrary = Disassembler.OmfLoader.LoadLibrary(fileName)
        library.ResolveAllSymbols()

        Dim dasm = New Disassembler.LibraryDisassembler(library)
        dasm.Analyze()
        Return library

    End Function

    Sub Main()

        Dim fileName As String

        'fileName = "..\..\..\..\Test\SLIBC7.LIB"
        'fileName = "..\..\..\..\Test\New\MLIBC7.LIB"
        'fileName = "..\..\..\..\Test\QB\BCOM45.LIB"
        fileName = "..\..\..\..\Test\QB\BRUN45.LIB"
        'fileName = "E:\Dev\Projects\DosDebugger\Test\Q.EXE"
        'fileName = "E:\Dev\Projects\DosDebugger\Test\New\NEWHELLO.EXE"
        'fileName = "E:\Dev\Projects\DosDebugger\Test\H.EXE"

        Dim program As Disassembler.Assembly = Nothing
        For i As Integer = 1 To 1
            If fileName.EndsWith(".LIB") Then
                program = DoOpenLibFile(fileName)
            Else
                program = DoOpenExeFile(fileName)
            End If
        Next
        Dim image = program.GetImage()

        ' Print statistics.

        Console.WriteLine("# instructions: {0}", image.Instructions.Count)
        Console.WriteLine("# instructions per block: {0:0.0}",
                          image.Instructions.Count / image.BasicBlocks.Count)
        Console.WriteLine()

        Console.WriteLine("# basic blocks: {0}", image.BasicBlocks.Count)
        Console.WriteLine("# control flow graph edges: {0}", image.BasicBlocks.ControlFlowGraph.Edges.Count)

        Dim totalSize As Long, maxSize As Long
        Dim maxInsnPerBlock As Integer = 0
        For Each block In image.BasicBlocks
            Dim n As Integer = 0
            For Each insn In block.GetInstructions(image)
                n += 1
            Next
            maxInsnPerBlock = Math.Max(maxInsnPerBlock, n)
            totalSize += block.Length
            maxSize = Math.Max(maxSize, block.Length)
        Next
        Console.WriteLine("Avg basic block size: {0:0.0}", totalSize / image.BasicBlocks.Count)
        Console.WriteLine("Max basic block size: {0}", maxSize)
        Console.WriteLine("Max # instructions per block: {0}", maxInsnPerBlock)
        Console.WriteLine()

#If False Then
        Dim path As String = "E:\Dev\Projects\DosDebugger\Test\QC"
        Dim files = System.IO.Directory.GetFiles(path)
        For Each file In files
            Console.WriteLine("Exporting {0}", file)
            ExportLibraryProcedures(file)
        Next
#End If

#If False Then
        Console.WriteLine("Procedures")
        Console.WriteLine("----------")
        'also write to file
        For Each procedure In image.Procedures
            Dim checksum = Disassembler.CodeChecksum.Compute(procedure, image)
            Dim s = String.Format("{0} {1} {2} {3}",
                                  image.FormatAddress(procedure.EntryPoint),
                                  procedure.Name,
                                  BytesToString(checksum.OpcodeChecksum).ToLowerInvariant(),
                                  procedure.Size)
            Console.WriteLine(s)
        Next
        Console.WriteLine()
#End If

#If True Then
        ' List all references of a symbol.
        Dim name As String = "b$HeaderErrStr"
        Console.WriteLine("Finding references of symbol {0}", name)
        If TypeOf program Is Disassembler.ObjectLibrary Then
            Dim library As Disassembler.ObjectLibrary = program
            For Each m As Disassembler.ObjectModule In library.Modules
                'Console.WriteLine("{0}", m)
                For Each externalSymbol In m.ExternalNames
                    If externalSymbol.Name = name Then
                        Console.WriteLine("{0} is referenced by {1}",
                                          name, m.Name)
                    End If
                Next
            Next
        End If

#End If

        Console.WriteLine("Press any key to continue")
        Console.ReadKey()

    End Sub

    Sub ExportAllLibraries(path As String)

    End Sub

    Sub ExportLibraryProcedures(fileName As String)

        Dim library = DoOpenLibFile(fileName)
        Dim image = library.Image

        Using writer = New System.IO.StreamWriter("E:\DDD-Procedures.txt", True)
            For Each procedure In image.Procedures
                Dim checksum = Disassembler.CodeChecksum.Compute(procedure, image)
                Dim s = String.Format("{0} {1} {2} {3} {4}",
                                      System.IO.Path.GetFileName(fileName),
                                      image.FormatAddress(procedure.EntryPoint),
                                      procedure.Name,
                                      BytesToString(checksum.OpcodeChecksum).ToLowerInvariant(),
                                      procedure.Size)
                'Console.WriteLine(s)
                writer.WriteLine(s)
            Next
        End Using

    End Sub

    Function BytesToString(bytes As Byte()) As String
        Dim soapBinary As New System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary(bytes)
        Return soapBinary.ToString()
    End Function

End Module