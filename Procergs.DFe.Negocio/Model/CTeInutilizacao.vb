Imports Procergs.DFe.Dto.DFeTiposBasicos
Imports System.Xml

Public Class CTeInutilizacao
    Inherits DFe
    Public Property AAInut As String = String.Empty
    Public Property NroSeqIni As String = String.Empty
    Public Property NroSeqFim As String = String.Empty
    Public ReadOnly Property AAAAInut As String
        Get
            Return (Val(AAInut) + 2000).ToString.Trim
        End Get
    End Property

    Public Property CampoID As String = String.Empty


    Public Sub New(xml As XmlDocument)
        MyBase.New(xml)
        XMLDFe = xml
        CarregaDadosInut()

    End Sub

    Protected Overrides Sub CarregaAutorizadosXML()
        Throw New NotImplementedException()
    End Sub

    Protected Overrides Sub CarregaDadosSubstituicao()
        Throw New NotImplementedException()
    End Sub

    Private Sub CarregaDadosInut()
        Try
            VersaoSchema = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/@versao", "CTe", Util.TpNamespace.CTe)
            CodUFAutorizacao = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/CTe:cUF/text()", "CTe", Util.TpNamespace.CTe)
            AAInut = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/CTe:ano/text()", "CTe", Util.TpNamespace.CTe)
            CodInscrMFEmitente = CStr("00000000000000" + Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/CTe:CNPJ/text()", "CTe", Util.TpNamespace.CTe)).PadLeft(14, "0")
            TipoInscrMFEmitente = TpInscrMF.CNPJ
            CodModelo = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/CTe:mod/text()", "CTe", Util.TpNamespace.CTe)
            Serie = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/CTe:serie/text()", "CTe", Util.TpNamespace.CTe)
            NroSeqIni = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/CTe:nCTIni/text()", "CTe", Util.TpNamespace.CTe)
            NroSeqFim = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/CTe:nCTFin/text()", "CTe", Util.TpNamespace.CTe)
            CampoID = Util.ExecutaXPath(XMLDFe, "/CTe:inutCTe/CTe:infInut/@Id", "CTe", Util.TpNamespace.CTe)
        Catch ex As DFeException
            Throw New DFeException("Erro no Schema: " & ex.Message, 215)
        End Try
    End Sub


End Class
