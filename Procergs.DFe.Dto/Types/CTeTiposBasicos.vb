Public Class CTeTiposBasicos
    Public Enum TpTomador As Byte
        Remetente = 0
        Expedidor = 1
        Recebedor = 2
        Destinatario = 3
        Outro = 4
        SemTomadorCTeOS = 5
    End Enum
    Public Enum TpEmiss As Byte
        Normal = 1
        ContingenciaOffLineGTVe = 2
        NFF = 3
        EPEC = 4
        FSDA = 5
        SVCRS = 7
        SVCSP = 8
    End Enum
    Public Enum TpIndIEToma As Byte
        ContribICMS = 1
        ContribIsento = 2
        NaoContrib = 9
    End Enum
    Public Enum TpICMS As Byte
        ICMS00 = 0
        ICMS20 = 1
        ICMS45 = 2
        ICMS60ST = 3
        ICMS90 = 4
        ICMSOutraUF = 5
        ICMSSN = 6
    End Enum
    Public Enum TpModal As Byte
        Rodoviario = 1
        Aereo = 2
        Aquaviario = 3
        Ferroviario = 4
        Dutoviario = 5
        Multimodal = 6
    End Enum
    Public Enum TpCTe As Byte
        Normal = 0
        Complementar = 1
        Substituicao = 3
        GTVe = 4
    End Enum
    Public Enum TpServico As Byte
        Normal = 0
        Subcontratacao = 1
        Redespacho = 2
        RedespachoIntermediario = 3
        ServicoVinculadoMultimodal = 4
        TransportePessoas = 6
        TransporteValores = 7
        ExcessoBagagem = 8
        GTVe = 9
    End Enum
    Public Enum TpSitCTe As Byte
        NaoCarregado = 0
        Autorizado = 1
        Denegado = 2
        Cancelado = 3
    End Enum
    Public Enum TpEvento As Integer
        CartaCorrecao = 110110
        Cancelamento = 110111
        EPEC = 110113
        RegistrosMultimodal = 110160
        ComprovanteEntrega = 110180
        CancelamentoComprovanteEntrega = 110181
        AutorizadoCTeComplementar = 240130
        CanceladoCTeComplementar = 240131
        CTeSubstituicao = 240140
        LiberacaoEPEC = 240160
        LiberacaoPrazoCancelamento = 240170
        GTVeAutorizadoCTeOS = 240180
        GTVeCanceladoCTeOS = 240181
        MDFeAutorizado = 310610
        MDFeCancelado = 310611
        RegistroPassagem = 310620
        Multimodal = 440160
        RegistroPassagemAuto = 510620
        PrestacaoServicoDesacordo = 610110
        CanceladoPrestacaoServicoDesacordo = 610111
        InsucessoEntrega = 110190
        CanceladoInsucessoEntrega = 110191
    End Enum
End Class
