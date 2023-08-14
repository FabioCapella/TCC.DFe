Public Class DFeTiposBasicos
    Public Enum TpInscrMF
        NaoInformado = 0
        CNPJ = 1
        CPF = 2
        Outros = 3
    End Enum
    Public Enum TpAmb
        Producao = 1
        Homologacao = 2
    End Enum
    Public Enum TpCodOrigProt
        NaoIniciado = -1
        RS = 1
        SiteDR = 2
        SVRS = 3
        SVCRS = 7
        SVCSP = 8
        ANMDFe = 9
        NaoAtendido = 0
    End Enum
    Public Enum TpDFe
        CTe = 57
        CTeOS = 67
        GTVe = 64
        BPe = 63
        NF3e = 66
        NFCOM = 62
        NFe = 55
        NFCe = 65
        MDFe = 58
    End Enum
    Public Enum TpSitAutorizacaoEvento
        Vinculado = 135
        NaoVinculado = 136
        AlertaSituacao = 134
    End Enum
    Public Enum TpSitEvento
        Ativo = 1
        Anulado = 2
    End Enum
    Public Enum TpAutorEve
        Emitente = 1
        FiscoEmitente = 2
        OutraSEFAZ = 3
        AmbienteNacional = 4
        ANMDFeRPAutomatico = 5
        Tomador = 6
        SVBA = 9
    End Enum
    Public Enum TpAmbAut
        Proprio = 1
        SVRS = 3
        SVSP = 5
        ANMDFe = 9
    End Enum
    Public Enum TpSVCCTe
        NaoUtilizaSVC = 0
        SVCRS = 7
        SVCSP = 8
    End Enum
    Public Enum TpCodUF As Byte
        Acre = 12
        Alagoas = 27
        Amapa = 16
        Amazonas = 13
        Bahia = 29
        Ceara = 23
        DistritoFederal = 53
        EspiritoSanto = 32
        Goias = 52
        Maranhao = 21
        MatoGrosso = 51
        MatoGrossoDoSul = 50
        MinasGerais = 31
        Para = 15
        Paraiba = 25
        Parana = 41
        Pernambuco = 26
        Piaui = 22
        RioDeJaneiro = 33
        RioGrandeDoNorte = 24
        RioGrandeDoSul = 43
        Rondonia = 11
        Roraima = 14
        SantaCatarina = 42
        SaoPaulo = 35
        Sergipe = 28
        Tocantins = 17
        Exterior = 99
        NaoInformado = 0
    End Enum
End Class