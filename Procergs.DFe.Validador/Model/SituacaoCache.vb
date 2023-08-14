Imports System.Text
''' <summary>
''' Classe responsável por carregar as situações de retorno com cStat e xMotivo
''' Adapta-se conforme o DFe passado como parâmetro
''' Implementa um singleton com cache e atualização a cada hora
''' </summary>
''' <remarks></remarks>
Public Class SituacaoCache

    Private Shared _DthProxSync As DateTime
    Private Shared _Sistema As String
    Private Shared _ListaTipoSituacao As List(Of TipoSituacaoDTO) = Nothing

    ' intervalo em segundos para realimentar DateTime com base no BD
    Private Const _IntervalSync As Double = 3600 ' 1 hora '

    'Padrão singleton
    Private Shared _Instance As SituacaoCache
    Public Shared ReadOnly Property Instance(sistema As String) As SituacaoCache
        Get
            If _Instance Is Nothing Then _Instance = New SituacaoCache(sistema)
            Return _Instance
        End Get
    End Property

    Public ReadOnly Property ListaTipoSituacao As List(Of TipoSituacaoDTO)
        Get
            If (DateTime.Now > _DthProxSync) Then
                _ListaTipoSituacao = DFeTipoSituacaoDAO.Listar(_Sistema)
                _DthProxSync = DateTime.Now.AddSeconds(_IntervalSync)
            End If

            Return _ListaTipoSituacao
        End Get
    End Property

    Public Sub New(sistema As String)
        Refresh(sistema)
    End Sub
    Public Shared Sub Refresh(sistema As String)
        _DthProxSync = DateTime.Now.AddSeconds(_IntervalSync)
        _Sistema = sistema
        'Carregar lista de situações 
        _ListaTipoSituacao = DFeTipoSituacaoDAO.Listar(sistema)
    End Sub

    Public Function ObterSituacao(codSituacao As Integer) As RetornoValidacaoDFe

        Dim tipoSit As TipoSituacaoDTO = ListaTipoSituacao.Find(Function(p As TipoSituacaoDTO) p.CodSituacao = codSituacao)
        If tipoSit Is Nothing Then
            Return New RetornoValidacaoDFe(codSituacao, String.Format("Situação Inexistente [{0}]", codSituacao.ToString))
        Else
            Return New RetornoValidacaoDFe(tipoSit.CodSituacao, tipoSit.Descricao)
        End If

    End Function

End Class