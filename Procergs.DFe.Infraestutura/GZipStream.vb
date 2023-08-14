Imports System.IO
Imports System.IO.Compression
Imports System.Text

''' <summary>
''' Compactação de conteúdo com uso do algoritmo GZip, namespace IO.Compression.
''' Utilização de propriedades do Sistema CT-e (Conhecimento de Transporte Eletrônica),
''' como exemplo, codificação UTF-8 para compactação e descompactação
''' </summary>
''' <remarks>
''' Autor: Fabio Capella
''' Data : 07/05/2008
''' 
''' Atualizado em:
''' 
''' </remarks>
Public Class GZipStream

    'Private lLen As Long

    'Public ReadOnly Property Length() As Long
    '    Get
    '        Return lLen
    '    End Get
    'End Property

    Public Shared Sub compacta(ByVal NomeArquivoOrigem As String,
                        Optional ByVal NomeArquivoDestino As String = "")

        Dim oStream As FileStream = Nothing
        Try
            oStream = New FileStream(NomeArquivoOrigem, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim buffer(oStream.Length - 1) As Byte
            Dim count As Integer = oStream.Read(buffer, 0, buffer.Length)
            oStream.Close()

            Dim oMs As New MemoryStream()
            Dim oGZip As New Compression.GZipStream(oMs, CompressionMode.Compress, True)
            oGZip.Write(buffer, 0, buffer.Length)
            oGZip.Close()
            If NomeArquivoDestino = "" Then
                NomeArquivoDestino = NomeArquivoOrigem & ".gz"
            End If

            'lLen = oMs.Length

            escrevaEmDisco(NomeArquivoDestino, oMs.ToArray, oMs.Length)

        Finally
            If Not (oStream Is Nothing) Then
                oStream.Close()
            End If
        End Try

    End Sub

    Public Shared Function compactaStringToArrayByte(ByVal sInf As String) As Byte()

        Dim oMs As MemoryStream = New MemoryStream()
        Dim aBuffer() As Byte

        Try
            aBuffer = ToArrayBytes(sInf)

            Dim oGZip As New Compression.GZipStream(oMs, CompressionMode.Compress, True)

            Try

                'lLen = aBuffer.Length

                oGZip.Write(aBuffer, 0, aBuffer.Length)
                oGZip.Flush()
                oGZip.Close()

            Finally

                If Not (oGZip Is Nothing) Then
                    oGZip.Close()
                End If

            End Try

            'lLen = oMs.Length

            Return oMs.ToArray

        Finally

            If Not (oMs Is Nothing) Then
                oMs.Close()
            End If

        End Try

    End Function

    Public Shared Sub descompacta(ByVal sNomeArquivoOrigem As String,
                           ByVal sNomeArquivoDestino As String)

        Dim oStream As FileStream = Nothing
        Try
            oStream = New FileStream(sNomeArquivoOrigem, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim oMs As New MemoryStream(oStream.Length)
            Dim oGUnzip As New Compression.GZipStream(oMs, CompressionMode.Decompress, True)

            Dim data(oStream.Length - 1) As Byte
            oStream.Read(data, 0, data.Length)
            oMs.Write(data, 0, data.Length)
            oMs.Position = 0

            Dim dataOutput() As Byte = ReadAllBytesOfStream(oGUnzip)

            escrevaEmDisco(sNomeArquivoDestino, dataOutput, dataOutput.Length)

            oGUnzip.Close()

        Finally

            If Not (oStream Is Nothing) Then
                oStream.Close()
            End If

        End Try

    End Sub

    Public Shared Function descompactaArrayBytesToString(ByVal aBuffer() As Byte) As String

        Dim oMs As New MemoryStream()

        Try

            oMs.Write(aBuffer, 0, aBuffer.Length)
            oMs.Seek(0, SeekOrigin.Begin)
            '*** Descompacta
            Dim oGUnZip As Stream = New Compression.GZipStream(oMs, CompressionMode.Decompress, True)
            Dim dataOutput() As Byte = ReadAllBytesOfStream(oGUnZip)
            oGUnZip.Close()

            'Desativado: Dim oASCII As Encoding = Encoding.ASCII 'GetEncoding("iso-8859-1")
            Dim oUTF_8 As Encoding = Encoding.UTF8 'GetEncoding("iso-8859-1")
            Return oUTF_8.GetString(dataOutput)

        Finally

            If Not (oMs Is Nothing) Then
                oMs.Close()
            End If

        End Try

    End Function

    Private Shared Function ReadAllBytesOfStream(ByVal s As Stream) As Byte()

        Dim buffer(10000) As Byte
        Dim bytesLidos As Integer = 0
        Using oMs As New MemoryStream()
            Do
                bytesLidos = s.Read(buffer, 0, 10000)
                oMs.Write(buffer, 0, bytesLidos)
            Loop Until bytesLidos <= 0
            Return oMs.ToArray()
        End Using

    End Function

    Public Shared Sub escrevaEmDisco(ByVal sNomeArquivoDestino As String,
                              ByVal aData() As Byte,
                              ByVal count As Integer)

        Dim oFs As New FileStream(sNomeArquivoDestino, FileMode.Create, FileAccess.Write)
        Try
            oFs.Write(aData, 0, count)
        Catch ex As Exception
            Throw ex
        Finally
            oFs.Close()
        End Try

    End Sub

    Private Shared Function ArrayOfByteToArrayOfSByte(ByVal b() As System.Byte) As System.SByte()

        Dim sb(b.Length - 1) As System.SByte
        Dim sb1 As System.SByte
        For i As Integer = 0 To b.Length - 1
            If (b(i) <= 127) Then
                sb1 = System.Convert.ToSByte(b(i))
            Else
                sb1 = System.Convert.ToSByte(b(i) - 256)
            End If
            sb(i) = sb1
        Next
        Return sb

    End Function

    ' Converte String em um array de bytes
    Private Shared Function ToArrayBytes(ByVal vStr As String) As Byte()

        ' Desativado: Dim encoding As New System.Text.ASCIIEncoding
        Dim encoding As New System.Text.UTF8Encoding
        ''Dim encoding As New System.Text.UnicodeEncoding
        Return encoding.GetBytes(vStr)

    End Function

End Class