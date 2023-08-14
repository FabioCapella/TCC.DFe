Public Class NF3eTiposBasicos
    Public Enum TpIndIEDest As Byte
        ContribICMS = 1
        ContribIsento = 2
        NaoContrib = 9
    End Enum
    Public Enum TpNF3e As Byte
        Normal = 1
        Substituicao = 2
        NormalComAjuste = 3
    End Enum
    Public Enum TpFase As Byte
        Monofasico = 1
        Bifasico = 2
        Trifasico = 3
    End Enum
    Public Enum TpAcesso As Byte
        Gerador = 0
        Cativo = 1
        Livre = 2
        ParcialmenteLivre = 3
        ConsumidorEspecial = 4
        ParcialmenteEspecial = 5
        Comunhao = 6
        Suprimento = 7
        Distribuidora = 8
    End Enum
    Public Enum TpEmissao As Byte
        Normal = 1
        Contigencia = 2
    End Enum
    Public Enum TpSitNF3e As Byte
        NaoCarregado = 0
        Autorizado = 1
        Ajustado = 2
        Cancelado = 3
        Substituido = 4
    End Enum
    Public Enum TpEvento As Integer
        Cancelamento = 110111
        LiberacaoPrazoCancelamento = 240170
        AutorizadaSubstituicao = 240140
    End Enum
End Class
