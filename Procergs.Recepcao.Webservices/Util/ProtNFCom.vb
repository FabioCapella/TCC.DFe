Imports Procergs.DFe.Negocio

<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/nfcom"),
System.Xml.Serialization.XmlRootAttribute([Namespace]:="http://www.portalfiscal.inf.br/nfcom", IsNullable:=False)>
Public Class protNFCom

    <System.Xml.Serialization.XmlElementAttribute("infProt")>
    Public infProt As TNFComProtocoloInfProt

    <System.Xml.Serialization.XmlElementAttribute("infFisco")>
    Public infFisco As TNFComProtocoloInfFisco

    <System.Xml.Serialization.XmlIgnoreAttribute()>
    Public infFiscoSpecified As Boolean

    <System.Xml.Serialization.XmlAttributeAttribute()>
    Public versao As String

End Class