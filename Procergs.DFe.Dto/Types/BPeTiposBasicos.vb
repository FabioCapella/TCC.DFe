Public Class BPeTiposBasicos
    Public Enum TpICMS As Byte
        ICMS00 = 0
        ICMS20 = 1
        ICMS45 = 2
        ICMS90 = 3
        ICMSSN = 4
    End Enum
    Public Enum TpModal As Byte
        Rodoviario = 1
        Aquaviario = 3
        Ferroviario = 4
    End Enum
    Public Enum TpPresenca As Byte
        Presencial_Nao_Embarcado = 1
        Nao_Presencial_Internet = 2
        Nao_Presencial_Tele = 3
        Entrega_Domicilio = 4
        Presencial_Embarcada = 5
        Nao_Presencial_Outros = 9
    End Enum
    Public Enum TpBPe As Byte
        Normal = 0
        Substituicao = 3
        TransporteMetropolitano = 4
    End Enum
    Public Enum TpSubst As Byte
        Remarcacao = 1
        Transferencia = 2
        Trans_Remarc = 3
    End Enum
    Public Enum TpDesconto As Byte
        TarifaPromocional = 1
        Idoso = 2
        Crianca = 3
        Deficiente = 4
        Estudante = 5
        AnimalDomestico = 6
        AcordoColetivo = 7
        ProfissionalDeslocamento = 8
        ProfissionalDaEmpresa = 9
        Jovem = 10
        Outros = 99
    End Enum
    Public Enum TpTrecho As Byte
        Normal = 1
        Inicial = 2
        Conexao = 3
    End Enum
    Public Enum TpServ As Byte
        Convencional_Com_Sanitario = 1
        Convencional_Sem_Sanitario = 2
        Semileito = 3
        Leito_Com_Ar = 4
        Leito_Sem_Ar = 5
        Executivo = 6
        Semiurbano = 7
        Longitudinal = 8
        Travessia = 9
        Cama = 10
    End Enum
    Public Enum TpSitBPe As Byte
        NaoCarregado = 0
        Autorizado = 1
        Cancelado = 3
        Substituido = 4
    End Enum
    Public Enum TpEmis As Byte
        Normal = 1
        Contingencia = 2
    End Enum
    Public Enum TpEvento As Integer
        Cancelamento = 110111
        NaoEmbarque = 110115
        AlteracaoPoltrona = 110116
        ExcessoBagagem = 110117
        Substituicao = 240140
        LiberacaoPrazoCancelamento = 240170
    End Enum
End Class
