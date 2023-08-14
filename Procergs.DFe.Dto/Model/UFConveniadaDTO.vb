Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class UFConveniadaDTO
    Public Shared aCOD_UF_IBGE() As String = New String() _
    {
           "11", "12", "13", "14", "15", "16", "17", "21", "22", "23", "24", "25", "26", "27", "28",
           "29", "31", "32", "33", "35", "41", "42", "43", "50", "51", "52", "53"
    }

    Public Shared aCOD_UF_SIGLA() As String = New String() _
    {
           "RO", "AC", "AM", "RR", "PA", "AP", "TO", "MA", "PI", "CE", "RN", "PB", "PE", "AL", "SE",
           "BA", "MG", "ES", "RJ", "SP", "PR", "SC", "RS", "MS", "MT", "GO", "DF"
    }


    Public Property CodUF As TpCodUF
    Public Property SiglaUF As String = String.Empty
    Public Property TipoAmbienteAutorizacaoCTe As TpAmbAut = TpAmbAut.Proprio
    Public Property TipoAmbienteAutorizacaoBPe As TpAmbAut = TpAmbAut.Proprio
    Public Property TipoAmbienteAutorizacaoMDFe As TpAmbAut = TpAmbAut.ANMDFe
    Public Property TipoAmbienteAutorizacaoNF3e As TpAmbAut = TpAmbAut.Proprio
    Public Property TipoAmbienteAutorizacaoNFCom As TpAmbAut = TpAmbAut.Proprio
    Public Property EmiteNFFTAC As Boolean = False
    Public Property IndEmiteCTe As Boolean = False
    Public Property IndEmiteMDFe As Boolean = False
    Public Property IndEmiteBPe As Boolean = False
    Public Property IndEmiteNF3e As Boolean = False
    Public Property IndEmiteNFCom As Boolean = False
    Public Property URLQRCodeCTe As String = String.Empty
    Public Property URLQRCodeNF3e As String = String.Empty
    Public Property URLQRCodeBPe As String = String.Empty
    Public Property URLQRCodeNFCom As String = String.Empty
    Public Property VlrLimiteNF3e As Double = 0
    Public Property VlrLimiteICMSNF3e As Double = 0
    Public Property VlrLimiteNFCom As Double = 0
    Public Property VlrLimiteICMSNFCom As Double = 0
    Public Property TipoSVC As TpSVCCTe = TpSVCCTe.NaoUtilizaSVC
    Public Property DthIniSVCCTe As DateTime
    Public Property DthFimSVCCTe As DateTime
    Public Property QtdHoraFuso As String = String.Empty
    Public Property QtdHoraVerao As String = String.Empty

    Public Shared Function ObterCodUF(ByVal vSigla_UF As String) As Byte
        Dim iIndex As Integer
        iIndex = Array.IndexOf(aCOD_UF_SIGLA, vSigla_UF)
        If iIndex >= 0 Then
            Return CInt(aCOD_UF_IBGE(iIndex))
        Else
            Return 0
        End If
    End Function

    Public Shared Function ObterSiglaUF(ByVal vCod_UF_IBGE As String) As String
        Dim iIndex As Integer
        iIndex = Array.IndexOf(aCOD_UF_IBGE, vCod_UF_IBGE)
        If iIndex >= 0 Then
            Return aCOD_UF_SIGLA(iIndex)
        Else
            Return 0
        End If
    End Function

End Class
