Imports System.Configuration

Public Enum TpAmbiente As Byte
    Desenvolvimento = 0
    '1 = deprecated entrou em desuso era o homolog empresas de desenvolvimento
    Homolog_Empresa = 2
    Producao = 3
    Site_DR_Dev = 4
    Site_DR_HMLE = 5
    Site_DR_PROD = 6
End Enum
Public Enum tpAmb As Byte
    Homologacao = 2
    Producao = 1
End Enum

'Public Enum TipoAmbiente As Byte
'    Producao = 1
'    Homologacao = 2
'End Enum

Public Class Conexao
    ''' <summary>
    ''' Para manter a lista de strings de conexão.
    ''' </summary>
    Private Shared _listaConexoes As Dictionary(Of String, DFeBancoDados)
    Private Shared _sistema As String
    Private Shared _ambienteBD As String
    Private Shared _tipoAmbiente As String

    Public Shared ReadOnly Property Sistema() As String
        Get
            If String.IsNullOrEmpty(_sistema) Then
                If String.IsNullOrEmpty(ConfigurationManager.AppSettings("sistema")) Then
                    Throw New Exception("O arquivo *.config não possui a variável 'sistema' definida no appSettings.")
                Else
                    _sistema = ConfigurationManager.AppSettings("sistema").ToString()
                End If
            End If
            Return _sistema
        End Get
    End Property
    Public Shared ReadOnly Property AmbienteBD() As TpAmbiente
        Get
            If String.IsNullOrEmpty(_ambienteBD) Then
                If String.IsNullOrEmpty(ConfigurationManager.AppSettings("ambiente")) Then
                    Throw New Exception("O arquivo *.config não possui a variável 'ambiente' definida no appSettings.")
                Else
                    _ambienteBD = ConfigurationManager.AppSettings("ambiente").ToString()
                End If
            End If
            Return CByte(_ambienteBD)

        End Get
    End Property

    Public Shared ReadOnly Property isSiteDR() As Boolean
        Get
            If AmbienteBD = TpAmbiente.Site_DR_Dev OrElse AmbienteBD = TpAmbiente.Site_DR_HMLE OrElse AmbienteBD = TpAmbiente.Site_DR_PROD Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public Shared ReadOnly Property TipoAmbiente() As tpAmb
        Get
            If Conexao.AmbienteBD = TpAmbiente.Producao OrElse
              Conexao.AmbienteBD = TpAmbiente.Desenvolvimento OrElse
              Conexao.AmbienteBD = TpAmbiente.Site_DR_Dev OrElse
              Conexao.AmbienteBD = TpAmbiente.Site_DR_PROD Then
                _tipoAmbiente = tpAmb.Producao
            Else
                _tipoAmbiente = tpAmb.Homologacao
            End If
            Return CByte(_tipoAmbiente)
        End Get
    End Property


    ''' <summary>
    ''' Construtor estático. Uma vez por APP_DOMAIN.
    ''' </summary>
    Shared Sub New()

        _listaConexoes = New Dictionary(Of String, DFeBancoDados)

        Select Case AmbienteBD

            Case TpAmbiente.Desenvolvimento
                _listaConexoes.Add("MDF", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_MDF"})
                _listaConexoes.Add("MDF_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_MDF_XML"})
                _listaConexoes.Add("MDF_DFE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_MDF_DFE"})
                _listaConexoes.Add("CTE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "CTE"})
                _listaConexoes.Add("CTE_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_CTE_XML"})
                _listaConexoes.Add("CTE_DFE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_CTE_DFE"})
                _listaConexoes.Add("CTE_CHV_SVC", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_CTE_CHV_SVC"})
                _listaConexoes.Add("SVD", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_SEF_DFE_SVD"})
                _listaConexoes.Add("BPE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_BPE"})
                _listaConexoes.Add("BPE_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_BPE_XML"})
                _listaConexoes.Add("BPE_DFE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_BPE_DFE"})
                _listaConexoes.Add("DFE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_SEF_DFE"})
                _listaConexoes.Add("DFE_2", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_SEF_DFE_2"})
                _listaConexoes.Add("DFE_LOG", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_SEF_DFE_LOG"})
                _listaConexoes.Add("DFE_PORTAL", New DFeBancoDados() With {.serverName = "SQL09.MSSQLSERVER-DES.procergs.reders\SQL09", .dataBaseName = "SF_SEF_DFE_PORTAL"})
                _listaConexoes.Add("DIFAL", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_DIFAL"})
                _listaConexoes.Add("ONE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_ONE"})
                _listaConexoes.Add("CMT", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_CMT"})
                _listaConexoes.Add("MPRS", New DFeBancoDados() With {.serverName = "sql09.mssqlserver-des.procergs.reders\SQL09", .dataBaseName = "SF_MPRS"})
                _listaConexoes.Add("NFG", New DFeBancoDados() With {.serverName = "sql02.mssqlserver-des.procergs.reders\SQL02", .dataBaseName = "SF_NFG"})
                _listaConexoes.Add("NFE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "NFE"})
                _listaConexoes.Add("NFE_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NFE_XML"})
                _listaConexoes.Add("NCE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NCE"})
                _listaConexoes.Add("NCE_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NCE_XML"})
                _listaConexoes.Add("SVP", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "NFE_SFV"})
                _listaConexoes.Add("SVP_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_SVP_XML"})
                _listaConexoes.Add("SCE", New DFeBancoDados() With {.serverName = "sql11.mssqlserver-des.procergs.reders\SQL11", .dataBaseName = "SF_SCE"})
                _listaConexoes.Add("SCE_XML", New DFeBancoDados() With {.serverName = "sql11.mssqlserver-des.procergs.reders\SQL11", .dataBaseName = "SF_SCE_XML"})
                _listaConexoes.Add("NF3E", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NF3E"})
                _listaConexoes.Add("NF3E_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NF3E_XML"})
                _listaConexoes.Add("NF3E_DFE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NF3E_DFE"})
                _listaConexoes.Add("NFF", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NFF"})
                _listaConexoes.Add("NFE_CPL", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NFE_CPL"})
                _listaConexoes.Add("SVP_CPL", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_SVP_CPL"})
                _listaConexoes.Add("NCE_CPL", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NCE_CPL"})
                _listaConexoes.Add("SCE_CPL", New DFeBancoDados() With {.serverName = "sql11.mssqlserver-des.procergs.reders\SQL11", .dataBaseName = "SF_SCE_CPL"})
                _listaConexoes.Add("SCE_DIST", New DFeBancoDados() With {.serverName = "sql11.mssqlserver-des.procergs.reders\SQL11", .dataBaseName = "SF_SCE_DIST"})
                _listaConexoes.Add("NF3E_COMPARTILHAMENTO", New DFeBancoDados() With {.serverName = "sql01.mssqlserver-des.procergs.reders\sql01", .dataBaseName = "SF_NF3E_COMPARTILHAMENTO"})
                _listaConexoes.Add("ONE_COMPARTILHAMENTO", New DFeBancoDados() With {.serverName = "sql10.mssqlserver-des.procergs.reders\sql10", .dataBaseName = "SF_ONE_COMPARTILHAMENTO"})
                _listaConexoes.Add("CTE_COMPARTILHAMENTO", New DFeBancoDados() With {.serverName = "sql01.mssqlserver-des.procergs.reders\sql01", .dataBaseName = "CTE_COMPARTILHAMENTO"})
                _listaConexoes.Add("MDF_COMPARTILHAMENTO", New DFeBancoDados() With {.serverName = "sql01.mssqlserver-des.procergs.reders\sql01", .dataBaseName = "SF_MDF_COMPARTILHAMENTO"})
                _listaConexoes.Add("NFE_COMPARTILHAMENTO", New DFeBancoDados() With {.serverName = "sql07.mssqlserver-des.procergs.reders\sql07", .dataBaseName = "NFE_COMPARTILHAMENTO"})
                _listaConexoes.Add("NCE_COMPARTILHAMENTO", New DFeBancoDados() With {.serverName = "sql07.mssqlserver-des.procergs.reders\sql07", .dataBaseName = "SF_NCE_COMPARTILHAMENTO"})
                _listaConexoes.Add("SAT_COMPARTILHADO", New DFeBancoDados() With {.serverName = "sql01.mssqlserver-des.procergs.reders\sql01", .dataBaseName = "SAT_COMPARTILHADO"})
                _listaConexoes.Add("SEF_CERTIF_DIGITAL", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SEF_CERTIF_DIGITAL"})
                _listaConexoes.Add("SEF_MASTER", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL08", .dataBaseName = "SEF_CADASTRO"})
                _listaConexoes.Add("SEF_WEB_SITE", New DFeBancoDados() With {.serverName = "SQL09.MSSQLSERVER-DES.procergs.reders\SQL09", .dataBaseName = "SEF_WEB_SITE"})
                _listaConexoes.Add("MDF_DR", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_MDF_DR"})
                _listaConexoes.Add("CTE_DR", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_CTE_DR"})
                _listaConexoes.Add("BPE_DR", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_BPE_DR"})
                _listaConexoes.Add("NF3E_DR", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NF3E_DR"})
                _listaConexoes.Add("NFCOM", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NFCOM"})
                _listaConexoes.Add("NFCOM_XML", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NFCOM_XML"})
                _listaConexoes.Add("NFCOM_DFE", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NFCOM_DFE"})
                _listaConexoes.Add("NFCOM_DR", New DFeBancoDados() With {.serverName = "sql06.mssqlserver-des.procergs.reders\SQL06", .dataBaseName = "SF_NFCOM_DR"})
            Case TpAmbiente.Homolog_Empresa
               ' Ocultado em vistas de serem dados sensiveis
            Case TpAmbiente.Producao
           ' Ocultado em vistas de serem dados sensiveis
          
            Case Else
                Throw New Exception(String.Format("Ocorreu um erro ao carregar a lista de strings de conexão. Ambiente da dll CON configurado incorretamente {0}.", AmbienteBD))
        End Select
    End Sub


    Public Shared Function CnxString(idBanco As String) As String

        If _listaConexoes.ContainsKey(idBanco.ToUpper) Then
            Return _listaConexoes(idBanco.ToUpper).connectionString
        End If

        Throw New Exception(String.Format("Ocorreu um erro ao obter conexão. Não existe a string de conexão para o sistema {0}.", idBanco))

    End Function

    Public Shared Function ObtemNomeBD(idBanco As String) As String

        If _listaConexoes.ContainsKey(idBanco.ToUpper) Then
            Return _listaConexoes(idBanco.ToUpper).dataBaseName
        End If

        Throw New Exception(String.Format("Ocorreu um erro ao obter Nome do Banco de Daos. Não existe o BD para o sistema {0}.", idBanco))

    End Function

    Private Class DFeBancoDados
        Public Property dataBaseName As String
        Public Property serverName As String

        Public ReadOnly Property connectionString As String
            Get
                Return "Data Source=" & serverName & ";Initial Catalog=" & dataBaseName & ";Integrated Security=SSPI;"
            End Get

        End Property
    End Class

End Class


