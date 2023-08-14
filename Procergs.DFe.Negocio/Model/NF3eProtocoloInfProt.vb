<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/nf3e")>
Public Class TNF3eProtocoloInfProt

    Public tpAmb As String

    Public verAplic As String

    Public chNF3e As String

    Public dhRecbto As String 'Date

    Public nProt As String 'System.UInt64

    <System.Xml.Serialization.XmlIgnoreAttribute()>
    Public nProtSpecified As Boolean

    Public digVal As String

    Public cStat As String 'System.UInt16

    Public xMotivo As String

    <System.Xml.Serialization.XmlAttributeAttribute(DataType:="ID")>
    Public Id As String

End Class