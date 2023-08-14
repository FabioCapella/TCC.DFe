''' <summary>
''' Classe para representar o ED Uso Indevido.
''' </summary>
Public Class UsoIndevidoDTO

    Public ReadOnly Property QtdLimiteStatus As Integer = 60
    Public ReadOnly Property QtdLimiteConsSit As Integer = 300
    Public ReadOnly Property QtdLimiteRetRecep As Integer = 150
    Public ReadOnly Property QtdLimiteEvento As Integer = 50
    Public ReadOnly Property QtdLimiteInut As Integer = 30
    Public ReadOnly Property QtdLimitePesqMenorPreco As Integer = 150
    Public ReadOnly Property QtdLimitePesqDiarioMenorPreco As Integer = 400
    Public ReadOnly Property QtdLimiteIntegrContab As Integer = 10
    Public ReadOnly Property QtdLimiteEncMDFe As Integer = 5

    Public Property DthIniMedicao As DateTime
    Public Property DthFimMedicao As DateTime
    Public Property SiglaSistema As String
    Public Property CodCnpjTransm As Long
    Public Property CodWS As Byte
    Public Property QtdUsoIndevido As Integer
    Public Property CtrDthInc As DateTime

    Public Function obterHorarioPadrao(Dth As DateTime) As DateTime
        Dim dthHorarioPadrao As DateTime
        Dim iMinutoPadrao As Byte = obterMinutoPadrao(Dth.Minute)
        If iMinutoPadrao > 0 Then
            dthHorarioPadrao = Convert.ToDateTime(Dth.Date.ToString("yyyy-MM-dd") & " " & Dth.Hour.ToString & ":" & iMinutoPadrao.ToString & ":00")
        Else
            dthHorarioPadrao = Dth.AddMinutes(60 - Dth.Minute)
            dthHorarioPadrao = Convert.ToDateTime(dthHorarioPadrao.Date.ToString("yyyy-MM-dd") & " " & dthHorarioPadrao.Hour & ":" & dthHorarioPadrao.Minute & ":00")
        End If
        Return dthHorarioPadrao
    End Function

    Private Shared Function obterMinutoPadrao(ByVal bytMin As Byte) As Byte
        Dim fatiaTempoPadraoMin As Byte = 5
        For i As Byte = 0 To ((60 / fatiaTempoPadraoMin) - 2)
            If bytMin >= (fatiaTempoPadraoMin * i) _
               And bytMin < (fatiaTempoPadraoMin * (i + 1)) Then
                Return fatiaTempoPadraoMin * (i + 1)
            End If
        Next
        Return 0

    End Function

End Class