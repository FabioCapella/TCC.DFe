Public Class FrotaANTTDTO
    Public Enum TpSitFrota
        Ativo = 1
        Inativo = 2
    End Enum

    Public Enum TpVeic
        Tracao = 1
        Rebocavel = 2
    End Enum
    Public Property CodIntFrota As Long
    Public Property CodIntTransp As Long
    Public Property Placa As String = String.Empty
    Public Property AnoVeiculo As Integer
    Public Property TaraVeiculo As Double
    Public Property QtdEixoVeiculo As Integer
    Public Property Marca As String = String.Empty
    Public Property RENAVAM As Long
    Public Property CodSitFrota As TpSitFrota = TpSitFrota.Ativo
    Public Property TipoVeiculo As TpVeic = TpVeic.Tracao
    Public Property Transportador As TransportadorDTO = Nothing
End Class
