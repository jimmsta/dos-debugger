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
        fileName = "E:\Dev\Projects\DosDebugger\Test\Q.EXE"

        Dim image As Disassembler.BinaryImage = Nothing
        For i As Integer = 1 To 1
            If fileName.EndsWith(".LIB") Then
                image = DoOpenLibFile(fileName)
            Else
                image = DoOpenExeFile(fileName)
            End If
        Next

        ' Print statistics.
        Console.WriteLine("# basic blocks: {0}", image.BasicBlocks.Count)
        Console.WriteLine("# control flow graph edges: {0}", image.BasicBlocks.ControlFlowGraph.Edges.Count)

        Dim totalSize As Long, maxSize As Long
        For Each block In image.BasicBlocks
            totalSize += block.Length
            maxSize = Math.Max(maxSize, block.Length)
        Next
        Console.WriteLine("Avg basic block size: {0}", Math.Ceiling(totalSize / image.BasicBlocks.Count))
        Console.WriteLine("Max basic block size: {0}", maxSize)

        Console.WriteLine("Press any key to continue")
        Console.ReadKey()

    End Sub

End Module