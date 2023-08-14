
''' <summary>
''' Classe para representar uma chave de acesso e facilitar o acesso as suas seções.
''' </summary>
Public Class ChaveAcesso

    Public Enum ModeloDFe As Integer
        NFe = 55
        NFCe = 65
        CTe = 57
        CTeOS = 67
        MDFe = 58
        BPe = 63
        NF3e = 66
        CTe_CTeOS = 5767
        CTe_CTeOS_GTVe = 576764
        GTVe = 64
        NFCom = 62
    End Enum

    Public Property MsgErro As String = ""

#Region "- Construtores"

    ''' <summary>
    ''' Construtor padrão.
    ''' </summary>
    Public Sub New(chaveAcesso As String)

        If String.IsNullOrEmpty(chaveAcesso) Then
            Throw New ArgumentException("Chave de acesso deve ser informada", "chaveAcesso")
        End If

        If chaveAcesso.Length <> 44 Then
            Throw New ArgumentException("Chave de acesso deve ter 44 digitos", "chaveAcesso")
        End If

        Me.mChaveAcesso = chaveAcesso

    End Sub

#End Region

#Region "- Membros"

    Private mChaveAcesso As String

    Private mUf As String
    Private mAAMM As String
    Private mCodInscrMFEmit As String
    Private mModelo As String
    Private mSerie As String
    Private mNumero As String
    Private mCodNumerico As String
    Private mTpEmis As String
    Private mDigitoVerificador As String
    Private mNroSiteAutoriz As String


#End Region

#Region "- Propriedades da Chave"

    ''' <summary>
    ''' Código da UF
    ''' </summary>    
    Public ReadOnly Property Uf() As String
        Get
            If String.IsNullOrEmpty(mUf) Then
                mUf = mChaveAcesso.Substring(0, 2)
            End If
            Return mUf
        End Get
    End Property

    ''' <summary>
    ''' Ano e Mês
    ''' </summary>    
    Public ReadOnly Property AAMM() As String
        Get
            If String.IsNullOrEmpty(mAAMM) Then
                mAAMM = mChaveAcesso.Substring(2, 4)
            End If
            Return mAAMM
        End Get
    End Property

    ''' <summary>
    ''' CNPJ do emitente.
    ''' </summary>    
    Public ReadOnly Property CodInscrMFEmit() As String
        Get
            If String.IsNullOrEmpty(mCodInscrMFEmit) Then
                mCodInscrMFEmit = mChaveAcesso.Substring(6, 14)
            End If
            Return mCodInscrMFEmit
        End Get
    End Property

    ''' <summary>
    ''' Modelo.
    ''' </summary>    
    Public ReadOnly Property Modelo() As String
        Get
            If String.IsNullOrEmpty(mModelo) Then
                mModelo = mChaveAcesso.Substring(20, 2)
            End If
            Return mModelo
        End Get
    End Property

    ''' <summary>
    ''' Serie.
    ''' </summary>    
    Public ReadOnly Property Serie() As String
        Get
            If String.IsNullOrEmpty(mSerie) Then
                mSerie = mChaveAcesso.Substring(22, 3)
            End If
            Return mSerie
        End Get
    End Property

    ''' <summary>
    ''' Número.
    ''' </summary>    
    Public ReadOnly Property Numero() As String
        Get
            If String.IsNullOrEmpty(mNumero) Then
                mNumero = mChaveAcesso.Substring(25, 9)
            End If
            Return mNumero
        End Get
    End Property

    ''' <summary>
    ''' Código numérico / Número aleatório.
    ''' </summary>    
    Public ReadOnly Property CodNumerico() As String
        Get
            If String.IsNullOrEmpty(mCodNumerico) Then
                If Modelo <> ModeloDFe.NF3e AndAlso Modelo <> ModeloDFe.NFCom Then
                    mCodNumerico = mChaveAcesso.Substring(35, 8)
                Else
                    mCodNumerico = mChaveAcesso.Substring(36, 7)
                End If
            End If
            Return mCodNumerico
        End Get
    End Property
    Public ReadOnly Property NroSiteAutoriz() As String
        Get
            If String.IsNullOrEmpty(mNroSiteAutoriz) Then
                If Modelo = ModeloDFe.NF3e OrElse Modelo = ModeloDFe.NFCom Then
                    mNroSiteAutoriz = mChaveAcesso.Substring(35, 1)
                End If
            End If
            Return mNroSiteAutoriz
        End Get
    End Property

    ''' <summary>
    ''' Tipo de emissão.
    ''' </summary>    
    Public ReadOnly Property TpEmis() As String
        Get
            If String.IsNullOrEmpty(mTpEmis) Then
                mTpEmis = mChaveAcesso.Substring(34, 1)
            End If
            Return mTpEmis
        End Get
    End Property

    ''' <summary>
    ''' Digito verificador.
    ''' </summary>    
    Public ReadOnly Property DigitoVerificador() As String
        Get
            If String.IsNullOrEmpty(mDigitoVerificador) Then
                mDigitoVerificador = mChaveAcesso.Substring(43, 1)
            End If
            Return mDigitoVerificador
        End Get
    End Property

    ''' <summary>
    ''' Própria chave de acesso.
    ''' </summary>    
    Public ReadOnly Property ChaveAcesso As String
        Get
            Return mChaveAcesso
        End Get
    End Property
#End Region

#Region "- Propriedades Auxiliares"

    ''' <summary>
    ''' Ano com 4 digitos.
    ''' </summary>
    ''' <value></value>    
    Public ReadOnly Property AAAA As String
        Get
            Return (Val(AAMM.Substring(0, 2)) + 2000).ToString.Trim
        End Get
    End Property

    ''' <summary>
    ''' Mês.
    ''' </summary>    
    Public ReadOnly Property MM As String
        Get
            Return (Val(AAMM.Substring(2, 2))).ToString.Trim
        End Get
    End Property

    ''' <summary>
    ''' Número da chave formatado.
    ''' </summary>    
    Public ReadOnly Property ChaveFormatada() As String
        Get
            Return String.Concat(
                   mChaveAcesso.Substring(0, 4),
                   " ",
                   mChaveAcesso.Substring(4, 4),
                   " ",
                   mChaveAcesso.Substring(8, 4),
                   " ",
                   mChaveAcesso.Substring(12, 4),
                   " ",
                   mChaveAcesso.Substring(16, 4),
                   " ",
                   mChaveAcesso.Substring(20, 4),
                   " ",
                   mChaveAcesso.Substring(24, 4),
                   " ",
                   mChaveAcesso.Substring(28, 4),
                   " ",
                   mChaveAcesso.Substring(32, 4),
                   " ",
                   mChaveAcesso.Substring(36, 4),
                   " ",
                   mChaveAcesso.Substring(40, 4)
               )

        End Get
    End Property

    ''' <summary>
    ''' Emitente CPF com IE.
    ''' </summary>    
    Public ReadOnly Property TipoInscrMFEmit As Byte
        Get
            If Modelo = 55 Then
                If ((TpEmis <> "3" AndAlso CInt(Serie) >= 910 AndAlso CInt(Serie) <= 969) OrElse (TpEmis = "3" AndAlso Numero.Substring(4, 1) = "2")) Then 'CPF
                    Return 2
                Else 'CNPJ
                    Return 1
                End If
            ElseIf Modelo = 57 Then
                Return IIf(TpEmis = "3", 2, 1) 'Se tipo emissão NFF retorna CPF
            ElseIf Modelo = 58 Then
                Return IIf((CInt(Serie) >= 920 AndAlso CInt(Serie) <= 969) OrElse TpEmis = "3", 2, 1) 'Se tipo emissão NFF retorna CPF
            Else
                Return 1
            End If
        End Get
    End Property

#End Region

#Region "Metodos Públicos"
    ''' <summary>
    ''' modVal => Modelo referencia para aplicar as regras de validação
    ''' Modelo => Modelo da chave de acesso que está sendo validada
    ''' </summary>  
    Public Function validaChaveAcesso(modVal As ModeloDFe) As Boolean

        If Not Util.DVChaveAcessoValido(ChaveAcesso) Then
            MsgErro = "[Motivo: DV inválido]"
            Return False
        End If

        If CInt(MM) = 0 Or CInt(MM) > 12 Then
            MsgErro = "[Motivo: Mês inválido]"
            Return False
        End If

        If CInt(Numero) = 0 Then
            MsgErro = "[Motivo: Número zerado]"
            Return False
        End If

        If UFConveniadaDTO.ObterSiglaUF(Uf) = "0" Then
            MsgErro = "[Motivo: UF inválida]"
            Return False
        End If

        If TipoInscrMFEmit = DFeTiposBasicos.TpInscrMF.CPF Then  'CPF
            If Not Util.ValidaDigitoCPFMF(CodInscrMFEmit) Then
                MsgErro = "[Motivo: CPF zerado ou inválido]"
                Return False
            End If
        Else  'CNPJ
            If Not Util.ValidaDigitoCNPJMF(CodInscrMFEmit) Then
                MsgErro = "[Motivo: CNPJ zerado ou inválido]"
                Return False
            End If
        End If

        If modVal <> ModeloDFe.CTe_CTeOS_GTVe AndAlso modVal <> ModeloDFe.CTe_CTeOS Then
            If Modelo <> modVal Then
                MsgErro = "[Motivo: Modelo diferente de " & modVal.ToString & "]"
                Return False
            End If
        Else
            If Modelo <> ModeloDFe.CTe AndAlso Modelo <> ModeloDFe.CTeOS AndAlso Modelo <> ModeloDFe.GTVe Then
                MsgErro = "[Motivo: Modelo diferente de 57 ou 67 ou 64]"
                Return False
            End If
        End If

        Select Case modVal
            Case ModeloDFe.BPe
                If CInt(AAAA) < 2017 Or CInt(AAAA) > DateTime.Now.Year Then
                    MsgErro = "[Motivo: Ano menor que 2017 ou maior que o atual]"
                    Return False
                End If

                If TpEmis <> "1" AndAlso TpEmis <> "2" Then
                    MsgErro = "[Motivo: Tipo de emissão inválido]"
                    Return False
                End If

            Case ModeloDFe.CTe, ModeloDFe.CTeOS, ModeloDFe.CTe_CTeOS, ModeloDFe.GTVe, ModeloDFe.CTe_CTeOS_GTVe
                If CInt(AAAA) < 2009 Or CInt(AAAA) > DateTime.Now.Year Then
                    MsgErro = "[Motivo: Ano menor que 2009 ou maior que o atual]"
                    Return False
                End If

                If Modelo = ModeloDFe.GTVe Then
                    If TpEmis <> "1" AndAlso TpEmis <> "2" AndAlso TpEmis <> "7" AndAlso TpEmis <> "8" Then
                        MsgErro = "[Motivo: Tipo de emissão inválido]"
                        Return False
                    End If
                Else
                    If TpEmis <> "1" AndAlso TpEmis <> "4" AndAlso TpEmis <> "5" AndAlso TpEmis <> "7" AndAlso TpEmis <> "8" AndAlso TpEmis <> "3" Then
                        MsgErro = "[Motivo: Tipo de emissão inválido]"
                        Return False
                    End If
                End If

            Case ModeloDFe.MDFe
                If CInt(AAAA) < 2012 Or CInt(AAAA) > DateTime.Now.Year Then
                    MsgErro = "[Motivo: Ano menor que 2012 ou maior que o atual]"
                    Return False
                End If

                If TpEmis <> "1" AndAlso TpEmis <> "2" AndAlso TpEmis <> "3" Then
                    MsgErro = "[Motivo: Tipo de emissão inválido]"
                    Return False
                End If

            Case ModeloDFe.NF3e
                If CInt(AAAA) < 2019 Or CInt(AAAA) > DateTime.Now.Year Then
                    MsgErro = "[Motivo: Ano menor que 2019 ou maior que o atual]"
                    Return False
                End If

                If TpEmis <> "1" AndAlso TpEmis <> "2" Then
                    MsgErro = "[Motivo: Tipo de emissão inválido]"
                    Return False
                End If
            Case ModeloDFe.NFCom
                If CInt(AAAA) < 2023 Or CInt(AAAA) > DateTime.Now.Year Then
                    MsgErro = "[Motivo: Ano menor que 2023 ou maior que o atual]"
                    Return False
                End If

                If TpEmis <> "1" AndAlso TpEmis <> "2" Then
                    MsgErro = "[Motivo: Tipo de emissão inválido]"
                    Return False
                End If
            Case ModeloDFe.NFe
                If CInt(AAAA) < 2005 Or CInt(AAAA) > DateTime.Now.Year Then
                    MsgErro = "[Motivo: Ano menor que 2005 ou maior que o atual]"
                    Return False
                End If

                If TpEmis < "1" OrElse TpEmis > "7" Then
                    MsgErro = "[Motivo: Tipo de emissão inválido]"
                    Return False
                End If

            Case ModeloDFe.NFCe
                If CInt(AAAA) < 2005 Or CInt(AAAA) > DateTime.Now.Year Then
                    MsgErro = "[Motivo: Ano menor que 2005 ou maior que o atual]"
                    Return False
                End If

                If TpEmis < "1" OrElse TpEmis > "7" Then
                    MsgErro = "[Motivo: Tipo de emissão inválido]"
                    Return False
                End If

        End Select

        MsgErro = ""
        Return True
    End Function

#End Region

End Class
