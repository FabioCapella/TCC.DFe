''' <summary>
''' Classe para representar a exceção quando não encontra o XML pelo XMLDecisionMDFeCTe.
''' </summary>
Public Class XmlNotFoundException : Inherits Exception

    Private mBDXmlAtivoMestreParam As Boolean

    ''' <summary>
    ''' Construtor.
    ''' </summary>
    Public Sub New(Msg As String, BDXmlAtivoMestreParam As Boolean)
        MyBase.New(Msg)
        mBDXmlAtivoMestreParam = BDXmlAtivoMestreParam
    End Sub

    ''' <summary>
    ''' Indica se o banco XML estava ativo na mestre param no momento da consulta.
    ''' </summary>
    Public ReadOnly Property BDXmlAtivoMestreParam() As Boolean
        Get
            Return mBDXmlAtivoMestreParam
        End Get
    End Property

End Class