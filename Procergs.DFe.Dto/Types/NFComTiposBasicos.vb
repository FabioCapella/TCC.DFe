Public Class NFComTiposBasicos
    Public Enum TpIndIEDest As Byte
        ContribICMS = 1
        ContribIsento = 2
        NaoContrib = 9
    End Enum
    Public Enum TpNFCom As Byte
        Normal = 0
        Substituicao = 3
        Ajuste = 4
    End Enum
    Public Enum TpFaturamento As Byte
        Normal = 0
        Centralizado = 1
        Cofaturamento = 2
    End Enum
    Public Enum TpServico As Byte
        Telefonia = 1
        Dados = 2
        TVAssinatura = 3
        ProvimentoAcessoInternet = 4
        Multimídia = 5
        Outros = 6
        Varios = 7
    End Enum
    Public Enum TpEmissao As Byte
        Normal = 1
        Contigencia = 2
    End Enum
    Public Enum TpSitNFCom As Byte
        NaoCarregado = 0
        Autorizado = 1
        Cancelado = 3
        Substituido = 4
    End Enum
    Public Enum TpEvento As Integer
        Cancelamento = 110111
        LiberacaoPrazoCancelamento = 240170
        AutorizadaSubstituicao = 240140
        AutorizadaAjuste = 240150
        CanceladaAjuste = 240151
        AutorizadaCofaturamento = 240160
        CanceladaCofaturamento = 240161
        SubstituídaCofaturamento = 240162
    End Enum
End Class
