Imports System.Text
''' <summary>
''' Classe responsável pela hora padrão do projeto CT-e
''' Obs.: Não há acesso ao OS (Observatório Nacional) e a hora padrão 
''' está representada pelo servidor de BD com o método getDate()
''' </summary>
''' <remarks></remarks>
Public Class DateTimeDFe

    Private m_parcelaSyncDTH As TimeSpan
    Private m_dth_prox_sync As DateTime

    Public m_dth_prox_sync_FUSO As DateTime
    Private m_Ult_UF_Pesq As String = ""
    Private m_Ult_Qtd_HORA_FUSO As Integer = -3

    ' intervalo em segundos para realimentar DateTime com base no BD
    Private Const m_dIntervalSync As Double = 60 ' 1 minuto '

    ' intervalo em segundos para realimentar DateTime com base no BD,
    ' considerando FUSO horário por UF
    Private Const m_dIntervalSync_FUSO As Double = 3600 ' 1 Hora 

    ''' <summary>
    ''' Relação de fusos horários em ralação à GMT
    ''' tendo como posição do array o código IBGE
    ''' </summary>
    ''' <remarks>Os valores abaixo são de inicialização, pois, o construtor da classe
    ''' irá atribuir os valores corretos vindo do BD, considerando inclusive a vigência 
    ''' do horário de verão.</remarks>
    ''' 
    Private m_aUF_Qtd_Hora_Fuso() As Integer = New Integer() _
     {
        -3, -3, -3, -3, -3, -3, -3, -3, -3, -3,
       -3, -3, -3, -3, -3, -3, -3, -3, -3, -3,
       -3, -3, -3, -3, -3, -3, -3, -3, -3, -3,
       -3, -3, -3, -3, -3, -3, -3, -3, -3, -3,
       -3, -3, -3, -3, -3, -3, -3, -3, -3, -3 _
       - 3, -3, -3, -3, -3, -3, -3, -3, -3, -3
      }


    ''' <summary>
    ''' Relação de fusos horários em ralação à GMT
    ''' tendo como posição do array o código IBGE da UF
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property aUF_Qtd_Hora_Fuso() As Integer()
        Get
            Return m_aUF_Qtd_Hora_Fuso
        End Get
    End Property

    ''' <summary>
    ''' Armazena a parcela de tempo a ser adicionada para correção e 
    ''' subsequente sincronismo entre a hora das máquinas servidoras do
    ''' projeto e o tempo padrão para o projeto (em milisegundos) 
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property parcelaSyncDTH() As TimeSpan
        Get
            Return m_parcelaSyncDTH
        End Get
    End Property

    'Padrão singleton
    Private Shared m_instance As DateTimeDFe
    Public Shared ReadOnly Property Instance As DateTimeDFe
        Get
            If m_instance Is Nothing Then m_instance = New DateTimeDFe
            Return m_instance
        End Get
    End Property


    Public Sub New()

        m_parcelaSyncDTH = getdate().Subtract(DateTime.Now)
        m_dth_prox_sync = DateTime.Now.AddSeconds(m_dIntervalSync)

        ' Pesquisa nova quantidade de horas em relação à GMT na tabela SEF_UNIDADE_FEDERACAO
        ' Efetuando carga do array de fusos horários
        cargaArray_FUSO_HORA()
        m_dth_prox_sync_FUSO = DateTime.Now.AddSeconds(m_dIntervalSync_FUSO)

    End Sub

    ''' <summary>
    ''' Fornece a data e hora atual, com base no servidor de Banco de Dados, aplicando e
    ''' corrigindo as diferenças de tempo entre os servidores do projeto com controle 
    ''' de domínios diferentes
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Somente irá pesquisar a data no BD (getdate()) se a data atual for maior que a próxima data de busca.
    ''' O intervalo de ida ao BD está definido no atributo m_dIntervalSync
    ''' </remarks>
    Public Function now() As DateTime
        Dim dth As DateTime
        Try
            ' 
            If (DateTime.Now > m_dth_prox_sync) Or (m_dIntervalSync = 0) Then
                ' Obter a hora padrão do projeto (atualmente em Banco de Dados)
                m_parcelaSyncDTH = getdate().Subtract(DateTime.Now)
                m_dth_prox_sync = DateTime.Now.AddSeconds(m_dIntervalSync)
            End If

        Catch ex As Exception
            DFeLogDAO.LogarEvento(Conexao.Sistema, "DatetimeDFe", Environment.MachineName, "Mantida a parcela de tempo de " & m_parcelaSyncDTH.ToString & " após ERRO ao tentar getDate() no BD." & ex.ToString, DFeLogDAO.TpLog.Erro, False)
        Finally

            ' Adiciona a parcela correspondente à diferença entre a hora da máquina
            ' e a hora âncora 
            dth = DateTime.Now.Add(m_parcelaSyncDTH)

        End Try

        Return dth

    End Function

    ''' <summary>
    ''' De acordo com a sigla UF passada como parâmetro e 
    ''' considerando o FUSO HORÁRIO, e ainda, o horário de verão brasileiro, 
    ''' fornece a data e hora atual, com base no servidor de Banco de Dados, aplicando e
    ''' corrigindo as diferenças de tempo entre os servidores do projeto com controle 
    ''' de domínios diferentes. 
    ''' </summary>
    ''' <returns>Caso o conteúdo do parâmetro seja RS, não será feito acesso ao BD e 
    ''' e terá o mesmo efeito no método Me.now() </returns>
    ''' <remarks>Somente irá pesquisar a data no BD (getdate()) se a data atual for maior que a próxima data de busca.
    ''' O intervalo de ida ao BD está definido no atributo m_dIntervalSync
    ''' </remarks>
    Public Function now(ByVal iCod_UF_IBGE As Integer) As DateTime
        Dim dth As DateTime
        Try
            ' FUSO HORÁRIO e HORÁRIO de VERÃO
            If (DateTime.Now > m_dth_prox_sync_FUSO) Or (m_dIntervalSync_FUSO = 0) Then
                ' Pesquisa nova quantidade de horas em relação à GMT na tabela SEF_UNIDADE_FEDERACAO
                ' Efetuando carga do array de fusos horários
                cargaArray_FUSO_HORA()
                m_dth_prox_sync_FUSO = DateTime.Now.AddSeconds(m_dIntervalSync_FUSO)
            End If
        Catch ex As Exception
            DFeLogDAO.LogarEvento(Conexao.Sistema, "DatetimeDFe", Environment.MachineName, "Mantida a parcela de tempo de " & m_parcelaSyncDTH.ToString & " após ERRO ao tentar getDate() no BD." & ex.ToString, DFeLogDAO.TpLog.Erro, False)
        Finally
            ' Busca a data e hora atual GMT, já considerando o ajuste pela hora âncora no BD
            ' Retornando a DTH considerando o FUSO e horário de verão da UF
            dth = DateTime.UtcNow.Add(m_parcelaSyncDTH).AddHours(m_aUF_Qtd_Hora_Fuso(iCod_UF_IBGE)) 'obterQTD_HORA_FUSO(sSigla_UF))
        End Try
        Return dth
    End Function

    Public Function utcNow(ByVal iCod_UF_IBGE As Integer) As String
        Dim dth As DateTime
        Try

            ' FUSO HORÁRIO e HORÁRIO de VERÃO
            If (DateTime.Now > m_dth_prox_sync_FUSO) Or (m_dIntervalSync_FUSO = 0) Then
                ' Pesquisa nova quantidade de horas em relação à GMT na tabela SEF_UNIDADE_FEDERACAO
                ' Efetuando carga do array de fusos horários
                cargaArray_FUSO_HORA()
                m_dth_prox_sync_FUSO = DateTime.Now.AddSeconds(m_dIntervalSync_FUSO)
            End If
        Catch ex As Exception
            DFeLogDAO.LogarEvento(Conexao.Sistema, "DatetimeDFe", Environment.MachineName, "Mantida a parcela de tempo de " & m_parcelaSyncDTH.ToString & " após ERRO ao tentar getDate() no BD." & ex.ToString, DFeLogDAO.TpLog.Erro, False)
        Finally
            ' Busca a data e hora atual GMT, já considerando o ajuste pela hora âncora no BD
            ' Retornando a DTH UTC ex.: 2012-08-17T17:18:00-03:00
            dth = DateTime.UtcNow.Add(m_parcelaSyncDTH).AddHours(m_aUF_Qtd_Hora_Fuso(iCod_UF_IBGE))
        End Try
        Return dth.ToString("yyyy-MM-ddTHH:mm:ss-") & (m_aUF_Qtd_Hora_Fuso(iCod_UF_IBGE) * -1).ToString.PadLeft(2, "0") + ":00"

    End Function

    Public Function toUTCFormat(ByVal iCod_UF_IBGE As Integer, dataRef As String) As String
        Return CDate(dataRef).ToString("yyyy-MM-ddTHH:mm:ss-") & (DateTimeDFe.Instance.obtem_FUSO_DATA_uf(Convert.ToDateTime(dataRef), UFConveniadaDTO.ObterSiglaUF(iCod_UF_IBGE)) * -1).ToString().PadLeft(2, "0") + ":00"
    End Function

    ''' <summary>
    ''' Método retorna o time stamp do BD que é referência do projeto.
    ''' Obs.: Enquanto não há uma definição sobre a captura de hora do OS 
    ''' (Observatório Nacional), a hora of13icial para o sistema CTe é a do 
    ''' servidor de Banco de Dados
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getdate() As DateTime

        Dim oCommand As New SqlClient.SqlCommand

        Dim sComSelect As String
        Try
            sComSelect = "SELECT GETDATE() "

            oCommand.CommandType = CommandType.Text
            oCommand.CommandText = sComSelect.ToString

            Return GerenciadorConexao.SEFDFECon.SQLObtem(oCommand).Item(0)
        Catch ex As Exception
            Throw ex
        Finally
            oCommand = Nothing
        End Try

    End Function

    Private Sub cargaArray_FUSO_HORA()
        Dim oDtbl1 As DataTable

        Dim sComSelect As StringBuilder
        Dim oCommand As New SqlClient.SqlCommand
        Try

            ' Busca período de Horario de Verao 
            Dim bHorario_Verao As Boolean = HorarioVerao()
            ' Busca lista de FUSOS horários por UF
            sComSelect = New StringBuilder("SELECT SIGLA_UF,COD_UF,QTD_HORA_FUSO,QTD_HORA_VERAO FROM SEF_UF_CONVENIADA (NOLOCK) ORDER BY COD_UF DESC ")

            oCommand.CommandType = CommandType.Text
            oCommand.CommandText = sComSelect.ToString

            oDtbl1 = GerenciadorConexao.SEFDFECon.SQLLista(oCommand)
            If (oDtbl1.Rows.Count > 0) Then

                For i As Byte = 0 To oDtbl1.Rows.Count - 1
                    If bHorario_Verao Then
                        m_aUF_Qtd_Hora_Fuso(If(oDtbl1.Rows(i).Item("COD_UF") Is DBNull.Value, 0, oDtbl1.Rows(i).Item("COD_UF"))) = IIf(oDtbl1.Rows(i).Item("QTD_HORA_VERAO") Is DBNull.Value, 0, oDtbl1.Rows(i).Item("QTD_HORA_VERAO"))
                    Else
                        m_aUF_Qtd_Hora_Fuso(If(oDtbl1.Rows(i).Item("COD_UF") Is DBNull.Value, 0, oDtbl1.Rows(i).Item("COD_UF"))) = IIf(oDtbl1.Rows(i).Item("QTD_HORA_FUSO") Is DBNull.Value, 0, oDtbl1.Rows(i).Item("QTD_HORA_FUSO"))
                    End If
                Next
            End If
        Finally
            oDtbl1 = Nothing
            oCommand = Nothing
        End Try

    End Sub
    Public Function obtem_FUSO_DATA_uf(ByVal dt As DateTime, ByVal sigla_UF As String) As Int16
        Dim oDtbl1 As DataTable
        Dim sComSelect As StringBuilder
        Dim oCommand As New SqlClient.SqlCommand
        Try
            If HorarioVerao() Then
                sComSelect = New StringBuilder("SELECT QTD_HORA_VERAO AS FUSO_HORARIO FROM SEF_UF_CONVENIADA WITH (NOLOCK) WHERE SIGLA_UF=''")
            Else
                sComSelect = New StringBuilder("SELECT QTD_HORA_FUSO AS FUSO_HORARIO FROM SEF_UF_CONVENIADA WITH (NOLOCK) WHERE SIGLA_UF=''")
            End If

            oCommand.CommandType = CommandType.Text
            oCommand.CommandText = sComSelect.ToString

            oDtbl1 = GerenciadorConexao.SEFDFECon.SQLLista(oCommand)

            Return Convert.ToInt16(oDtbl1.Rows(0).Item(0))

        Catch ex As Exception
            DFeLogDAO.LogarEvento(Conexao.Sistema, "DatetimeDFe", Environment.MachineName, "Erro ao obter o FUSO da data para a UF " & sigla_UF & "Data: " & dt.ToString & " " & ex.ToString, DFeLogDAO.TpLog.Erro, False)
        Finally
            oDtbl1 = Nothing

        End Try

    End Function


    Private Function HorarioVerao() As Boolean
        Dim oDtbl1 As DataTable
        Dim oCommand As New SqlClient.SqlCommand

        Dim sComSelect As StringBuilder
        Try
            ' Pesquisa se o Horário de Verão brasileiro está em vigor
            sComSelect = New StringBuilder("SELECT TEX_PARAM, TEX_PARAM_2 from SEF_MESTRE_PARAM WHERE NOME_MESTRE = 'STATUS_HORARIO_VERAO' AND CONVERT(DATETIME,TEX_PARAM_2) >= GETDATE() AND CONVERT(DATETIME, TEX_PARAM) <= GETDATE()")

            oCommand.CommandType = CommandType.Text
            oCommand.CommandText = sComSelect.ToString

            oDtbl1 = GerenciadorConexao.SEFDFECon.SQLLista(oCommand)
            If (oDtbl1.Rows.Count > 0) Then
                Return True
            End If
            Return False
        Finally
            oDtbl1 = Nothing
            oCommand = Nothing
        End Try

    End Function

End Class