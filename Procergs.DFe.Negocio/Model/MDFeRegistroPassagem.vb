Imports Procergs.DFe.Dto.DFeTiposBasicos
Public MustInherit Class MDFeRegistroPassagem

    Public Property Placa As String = String.Empty
    Public Property TipoSentido As String = String.Empty
    Public Property TipoTransmissao As String = String.Empty
    Public Property DthRegistroPassagem As DateTime?
    Public Property CodUFPassagem As TpCodUF
    Public Property NroLatitude As String = String.Empty
    Public Property NroLongitude As String = String.Empty

    Public ReadOnly Property DthRegistroPassagemUTC As DateTime
        Get
            If DthRegistroPassagem.HasValue Then
                Return Convert.ToDateTime(DthRegistroPassagem).ToUniversalTime
            Else
                Return Nothing
            End If
        End Get
    End Property

End Class
Public Class MDFeRegistroPassagemPosto
    Inherits MDFeRegistroPassagem
    Public Property CPFFuncionarioPostoFiscal As String = String.Empty
    Public Property NomeFuncionarioPostoFiscal As String = String.Empty
    Public Property CodUnidFiscal As String = String.Empty
    Public Property NomeUnidFiscal As String = String.Empty
    Public Property Observacao As String = String.Empty
End Class
Public Class MDFeRegistroPassagemAutomatico
    Inherits MDFeRegistroPassagem
    Public Property NSU As String = String.Empty
    Public Property IdEqp As String = String.Empty
    Public Property NomeEqp As String = String.Empty
    Public Property TipoEqp As String = String.Empty
    Public Property PesoBrutoTot As String = String.Empty
End Class