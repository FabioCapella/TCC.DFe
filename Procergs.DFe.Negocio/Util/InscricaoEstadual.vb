' Para uso da DLL de validação IE - Sintegra
''' <summary>
''' Validação Inscricao - Projeto CT-e Nota Fiscal Eletrônica
''' </summary>
''' <remarks></remarks>
Public Class InscricaoEstadual

    Public Shared Function inscricaoValidaRS(ByVal inscricao As String) As Boolean

        Try
            Dim digito As Char
            Dim insc As String

            If Not IsNumeric(inscricao) Then
                Return False
            End If

            If (inscricao.Length < 8) Then ''Or (inscricao.Length > 10) Then
                Return False
            End If

            insc = Microsoft.VisualBasic.Right("0000000000" & inscricao, 10)

            ' Os 3 primeiros dígitos representam o código município
            If (Int(insc.Substring(0, 3)) < 1) Or (Int(insc.Substring(0, 3)) > 497) Then
                Return False
            End If
            ' O quarto dígito repesenta o tipo de contribuinte
            If ((Int(insc.Substring(3, 1)) > 4) And (Int(insc.Substring(3, 1)) < 8)) Then
                Return False
            End If

            digito = Util.GetDigitoMod11Ext(insc.Substring(0, 9), 9, 9)
            If digito <> insc.Chars(9) Then
                Return False
            Else
                Return True
            End If

        Catch ex As Exception
            Return False
        End Try

    End Function

End Class