Public Class MDFeTiposBasicos
    Public Enum TpModal As Byte
        Rodoviario = 1
        Aereo = 2
        Aquaviario = 3
        Ferroviario = 4
    End Enum
    Public Enum TpTransp As Byte
        NaoPreenchido = 0
        ETC = 1
        TAC = 2
        CTC = 3
    End Enum
    Public Enum TpEmit As Byte
        ServTransp = 1
        CargaPropria = 2
        CTeGlobalizado = 3
    End Enum
    Public Enum TpEmiss As Byte
        Normal = 1
        Contingencia = 2
        NFF = 3
    End Enum
    Public Enum TpSitMDFe As Byte
        NaoCarregado = 0
        Autorizado = 1
        Cancelado = 3
        Encerrado = 4
    End Enum
    Public Enum TpEvento As Integer
        Cancelamento = 110111
        Encerramento = 110112
        InclusaoCondutor = 110114
        InclusaoDFe = 110115
        PagamentoOperacaoTransporte = 110116
        ConfirmacaoServicoTransporte = 110117
        AlteracaoPagamentoServicoTransporte = 110118
        LiberacaoPrazoCancelamento = 240170
        EncerramentoFisco = 310112
        RegistroPassagem = 310620
        RegistroPassagemAutomatico = 510620
        RegistroCessaoOnusGravame = 900120
        CancelamentoRegistroCessaoOnusGravame = 900121
        PagamentoTotalDe = 900134
        CancelamentoPagamentoTotalDe = 900135
        BaixaAtivoFinanceiroEmGarantia = 900136
        CancelamentoBaixaAtivoFinanceiroEmGarantia = 900137
    End Enum
    Public Enum TpEncerramentoFisco As Byte
        Webservice = 1
        Extranet = 2
        Automatico = 3
    End Enum
End Class
