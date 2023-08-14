Imports Procergs.DFe.Negocio

Imports System.Xml.Serialization

<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/mdfe"), _
System.Xml.Serialization.XmlRootAttribute([Namespace]:="http://www.portalfiscal.inf.br/mdfe", IsNullable:=False)> _
Public Class protMDFe

    <System.Xml.Serialization.XmlElementAttribute("infProt")>
    Public infProt As TMDFeProtocoloInfProt  'MDFe

    <System.Xml.Serialization.XmlAttributeAttribute()> _
    Public versao As String

End Class