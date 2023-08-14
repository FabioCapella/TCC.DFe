
Imports System.Xml.Serialization
Imports Procergs.DFe.Negocio

<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/nf3e"),
System.Xml.Serialization.XmlRootAttribute([Namespace]:="http://www.portalfiscal.inf.br/nf3e", IsNullable:=False)>
Public Class protNF3e

    <System.Xml.Serialization.XmlElementAttribute("infProt")>
    Public infProt As TNF3eProtocoloInfProt

    <System.Xml.Serialization.XmlElementAttribute("infFisco")>
    Public infFisco As TNF3eProtocoloInfFisco

    <System.Xml.Serialization.XmlIgnoreAttribute()>
    Public infFiscoSpecified As Boolean

    <System.Xml.Serialization.XmlAttributeAttribute()> _
    Public versao As String

End Class