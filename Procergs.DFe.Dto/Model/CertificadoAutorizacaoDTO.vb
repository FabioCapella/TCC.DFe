Imports Procergs.DFe.Dto.DFeTiposBasicos
Public Class CertificadoAutorizacaoDTO
    Public Property CodIntCertificado As Integer
    Public Property NomeEmissorAC As String = String.Empty
    Public Property NomeEmpresa As String = String.Empty
    Public Property NomeTitular As String = String.Empty
    Public Property CodInscrMFCertificado As Long
    Public Property AKIAutCertificado As String = String.Empty
    Public Property AKIAutCertificadoDig As String = String.Empty
    Public Property Serie As String = String.Empty
    Public Property ICPBrasil As Boolean?
    Public Property UsoTransmissao As Boolean?
    Public Property UsoAssinatura As Boolean?
    Public Property Revogado As Boolean?
    Public Property DthIniVigencia As DateTime
    Public Property DthFimVigencia As DateTime
    Public Property DthNextUpdateLCR As DateTime?
    Public Property CertRaw64 As String = String.Empty
    Public Property CtrDthInc As DateTime
    Public Property CtrDthAtu As DateTime
    Public Property Thumbprint As String = String.Empty
    Public Property CodigoRejeicao As Int16?
    Public Property DthUltimoUsoIndevidoCTe As DateTime?
    Public Property DthUltimoUsoIndevidoMDFe As DateTime?
    Public Property DthUltimoUsoIndevidoBPe As DateTime?
    Public Property DthFimGravacaoCaixa As DateTime?
    Public Property TipoInscrMFCertificado As TpInscrMF
    Public Property CodIntHLCR As Integer?
    Public Property DthUltimoUsoIndevidoNF3e As DateTime?
    Public Property DthUltimoUsoIndevidoNFCOM As DateTime?

End Class
