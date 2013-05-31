Module Module1

    Function DoOpenExeFile(fileName As String) As Disassembler.BinaryImage

        Dim executable As New Disassembler.Executable(fileName)
        Dim dasm As New Disassembler.ExecutableDisassembler(executable)
        dasm.Analyze()
        Return executable.Image

    End Function

    Function DoOpenLibFile(fileName As String) As Disassembler.BinaryImage

        Dim library As Disassembler.ObjectLibrary = Disassembler.OmfLoader.LoadLibrary(fileName)
        library.ResolveAllSymbols()

        Dim dasm = New Disassembler.LibraryDisassembler(library)
        dasm.Analyze()
        Return library.Image

    End Function

    Sub Main()

        Dim fileName As String

        'fileName = "..\..\..\..\Test\SLIBC7.LIB"
        fileName = "..\..\..\..\Test\New\MLIBC7.LIB"
        'fileName = "E:\Dev\Projects\DosDebugger\Test\Q.EXE"
        'fileName = "E:\Dev\Projects\DosDebugger\Test\New\NEWHELLO.EXE"
        'fileName = "E:\Dev\Projects\DosDebugger\Test\H.EXE"

        Dim image As Disassembler.BinaryImage = Nothing
        For i As Integer = 1 To 1
            If fileName.EndsWith(".LIB") Then
                image = DoOpenLibFile(fileName)
            Else
                image = DoOpenExeFile(fileName)
            End If
        Next

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

        Console.WriteLine("Procedures")
        Console.WriteLine("----------")
        'also write to file
        Using file = New System.IO.StreamWriter("E:\TestDDD-Procedures.txt")
            For Each procedure In image.Procedures
                Dim checksum = Disassembler.CodeChecksum.Compute(procedure, image)
                Dim s = String.Format("{0} {1} {2} {3}",
                                      image.FormatAddress(procedure.EntryPoint),
                                      procedure.Name,
                                      BytesToString(checksum.OpcodeChecksum).ToLowerInvariant(),
                                      procedure.Size)
                Console.WriteLine(s)
                file.WriteLine(s)
            Next
        End Using

        Console.WriteLine()

        Console.WriteLine("Press any key to continue")
        Console.ReadKey()

    End Sub

    Function BytesToString(bytes As Byte()) As String
        Dim soapBinary As New System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary(bytes)
        Return soapBinary.ToString()
    End Function

End Module