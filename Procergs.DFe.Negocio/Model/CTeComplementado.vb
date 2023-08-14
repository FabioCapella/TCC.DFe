Public Class CTeComplementado
    Public Sub New(chaveAcessoComplementado As ChaveAcesso, complementadoEncontrado As Boolean, Optional cteRefComplementado As CTeDTO = Nothing)
        Me.ChAcessoComplementado = chaveAcessoComplementado
        Me.ComplementadoEncontrado = complementadoEncontrado
        Me.CTeRefComplementado = cteRefComplementado
    End Sub

    Public Property ChAcessoComplementado As ChaveAcesso
    Public Property ComplementadoEncontrado As Boolean = False
    Public Property CTeRefComplementado As CTeDTO = Nothing
End Class
