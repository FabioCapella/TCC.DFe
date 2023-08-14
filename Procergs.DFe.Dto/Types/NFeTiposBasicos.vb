Public Class NFeTiposBasicos
    Public Enum TpEmiss As Byte
        Normal = 1
        FSIA = 2
        NFF = 3
        EPEC = 4
        FSDA = 5
        SVCRS = 7
        SVCAN = 6
    End Enum

    Public Enum TpIndIEToma As Byte
        ContribICMS = 1
        ContribIsento = 2
        NaoContrib = 9
    End Enum
    Public Enum TpSitNFe As Byte
        NaoCarregado = 0
        Autorizado = 1
        Denegado = 2
        Cancelado = 3
    End Enum
End Class
