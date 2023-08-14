Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class TransportadorDTO

    Public Enum TpCateg
        ETC = 1
        CTC = 2
        TAC = 3
    End Enum
    Public Enum TpSitTransp
        Ativo = 1
        Inativo = 2
    End Enum

    Public Property CodIntTransp As Long
    Public Property TipoInscrMFTransp As TpInscrMF = TpInscrMF.NaoInformado
    Public Property CodInscrMFTransp As Long
    Public Property NomeTransp As String = String.Empty
    Public Property RNTRC As Long
    Public Property TipoCategoria As TpCateg = TpCateg.ETC
    Public Property IndExigeCIOT As Boolean = False
    Public Property CodSitTransp As TpSitTransp = TpSitTransp.Ativo
End Class
