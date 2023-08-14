Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class ContribuinteDTO
    Public Property CodInscrMF As Long
    Public Property TipoInscrMF As TpInscrMF = TpInscrMF.CNPJ
    Public Property CodUF As TpCodUF
    Public Property CodIE As Long
    Public Property CodSitIE As Byte
    Public Property TipoIE As Byte
    Public Property IndCredenCTeRodo As Boolean = False
    Public Property IndCredenCTeAero As Boolean = False
    Public Property IndCredenCTeAqua As Boolean = False
    Public Property IndCredenCTeFerro As Boolean = False
    Public Property IndCredenCTeDuto As Boolean = False
    Public Property IndCredenCTeMultiModal As Boolean = False
    Public Property IndCredenCTeOS As Boolean = False
    Public Property IndCredenBPe As Boolean = False
    Public Property IndCredenBPeTM As Boolean = False
    Public Property IndCredenNF3e As Boolean = False
    Public Property IndCredenNFe As Boolean = False
    Public Property IndCredenNFCe As Boolean = False
    Public Property IndCredenNFCOM As Boolean = False
    Public Property DthExc As DateTime = Nothing

End Class
