<System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.portalfiscal.inf.br/nfcom")>
Public Class TNFComProtocoloInfEvento
    Public tpAmb As String

    Public verAplic As String

    Public cOrgao As String

    Public cStat As String 'System.UInt16

    Public xMotivo As String

    Public chNFCom As String

    Public tpEvento As String

    Public xEvento As String

    Public nSeqEvento As String

    Public dhRegEvento As String 'Date

    Public nProt As String 'System.UInt64

    <System.Xml.Serialization.XmlIgnoreAttribute()>
    Public nProtSpecified As Boolean

    <System.Xml.Serialization.XmlAttributeAttribute(DataType:="ID")>
    Public Id As String
End Class