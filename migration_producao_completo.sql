-- ============================================================
-- SCRIPT DE MIGRAÇÃO — GestAluguel — Produção
-- Gerado em: 06/05/2026
-- 
-- COMO USAR:
--   1. Abra este script no SSMS ou Azure Data Studio apontando
--      para o banco de PRODUÇÃO.
--   2. Execute tudo de uma vez (F5).
--   3. O script é IDEMPOTENTE — verifica antes de aplicar cada
--      migration; pode rodar sem medo mesmo se algumas já foram.
--
-- Migrations incluídas (em ordem):
--   [1] 20260429125932_MigracaoInicial
--   [2] 20260429131226_AdicionarWalletIdAsaasNaConfiguracao
--   [3] 20260429220437_AdicionarDataNascimentoInquilino
--   [4] 20260430000404_AdicionarHostECamposMorador
--   [5] 20260506043939_AdicionarValorGaragemEWhatsAppPix
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;   -- rollback automático se qualquer instrução falhar

-- Garante que a tabela de histórico do EF existe
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId]    nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '>> Tabela __EFMigrationsHistory criada.';
END

-- ============================================================
-- [1] MigracaoInicial
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429125932_MigracaoInicial')
BEGIN
    PRINT '>> Aplicando: MigracaoInicial...';

    BEGIN TRANSACTION;

    CREATE TABLE [Apartamentos] (
        [Id]          uniqueidentifier NOT NULL,
        [Numero]      nvarchar(20)     NOT NULL,
        [Bloco]       nvarchar(10)     NULL        DEFAULT N'',
        [Ocupado]     bit              NOT NULL    DEFAULT 0,
        [CriadoEm]    datetime2        NOT NULL,
        [AtualizadoEm] datetime2       NULL,
        CONSTRAINT [PK_Apartamentos] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Apartamentos_Numero_Bloco]
        ON [Apartamentos] ([Numero], [Bloco])
        WHERE [Bloco] IS NOT NULL;

    CREATE TABLE [Configuracoes] (
        [Id]          uniqueidentifier NOT NULL,
        [KwhValor]    decimal(18,4)    NOT NULL,
        [ValorAgua]   decimal(18,2)    NOT NULL,
        [CriadoEm]    datetime2        NOT NULL,
        [AtualizadoEm] datetime2       NULL,
        CONSTRAINT [PK_Configuracoes] PRIMARY KEY ([Id])
    );

    -- Registro singleton de configuração
    INSERT INTO [Configuracoes] ([Id], [AtualizadoEm], [CriadoEm], [KwhValor], [ValorAgua])
    VALUES (N'00000000-0000-0000-0000-000000000001', NULL, '2025-01-01T00:00:00.000Z', 0.0, 0.0);

    CREATE TABLE [GastosManutencao] (
        [Id]            uniqueidentifier NOT NULL,
        [ApartamentoId] uniqueidentifier NOT NULL,
        [Descricao]     nvarchar(500)    NOT NULL,
        [Valor]         decimal(18,2)    NOT NULL,
        [Data]          date             NOT NULL,
        [Observacao]    nvarchar(1000)   NULL,
        [CriadoEm]      datetime2        NOT NULL,
        [AtualizadoEm]  datetime2        NULL,
        CONSTRAINT [PK_GastosManutencao] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GastosManutencao_Apartamentos_ApartamentoId]
            FOREIGN KEY ([ApartamentoId]) REFERENCES [Apartamentos] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_GastosManutencao_ApartamentoId] ON [GastosManutencao] ([ApartamentoId]);

    CREATE TABLE [Inquilinos] (
        [Id]                     uniqueidentifier NOT NULL,
        [NomeCompleto]           nvarchar(200)    NOT NULL,
        [Cpf]                    nvarchar(11)     NOT NULL,
        [QuantidadeMoradores]    int              NOT NULL,
        [DataEntrada]            date             NOT NULL,
        [DataVencimentoContrato] date             NOT NULL,
        [ValorAluguel]           decimal(18,2)    NOT NULL,
        [Garagem]                decimal(18,2)    NOT NULL DEFAULT 0,
        [DiasAlertaVencimento]   nvarchar(max)    NOT NULL,
        [ApartamentoId]          uniqueidentifier NOT NULL,
        [CriadoEm]               datetime2        NOT NULL,
        [AtualizadoEm]           datetime2        NULL,
        CONSTRAINT [PK_Inquilinos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Inquilinos_Apartamentos_ApartamentoId]
            FOREIGN KEY ([ApartamentoId]) REFERENCES [Apartamentos] ([Id]) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_Inquilinos_Cpf]          ON [Inquilinos] ([Cpf]);
    CREATE INDEX        [IX_Inquilinos_ApartamentoId] ON [Inquilinos] ([ApartamentoId]);

    CREATE TABLE [ContratosInquilino] (
        [Id]                   uniqueidentifier NOT NULL,
        [InquilinoId]          uniqueidentifier NOT NULL,
        [NomeOriginalArquivo]  nvarchar(260)    NOT NULL,
        [CaminhoArquivo]       nvarchar(500)    NOT NULL,
        [TipoConteudo]         nvarchar(100)    NOT NULL,
        [TamanhoBytes]         bigint           NOT NULL,
        [Descricao]            nvarchar(500)    NULL,
        [CriadoEm]             datetime2        NOT NULL,
        [AtualizadoEm]         datetime2        NULL,
        CONSTRAINT [PK_ContratosInquilino] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContratosInquilino_Inquilinos_InquilinoId]
            FOREIGN KEY ([InquilinoId]) REFERENCES [Inquilinos] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ContratosInquilino_InquilinoId] ON [ContratosInquilino] ([InquilinoId]);

    CREATE TABLE [Dependentes] (
        [Id]            uniqueidentifier NOT NULL,
        [NomeCompleto]  nvarchar(200)    NOT NULL,
        [Cpf]           nvarchar(11)     NOT NULL,
        [Rg]            nvarchar(20)     NULL,
        [OrgaoEmissor]  nvarchar(20)     NULL,
        [DataNascimento] date            NOT NULL,
        [Telefone]      nvarchar(20)     NULL,
        [EstadoCivil]   int              NOT NULL,
        [InquilinoId]   uniqueidentifier NOT NULL,
        [CriadoEm]      datetime2        NOT NULL,
        [AtualizadoEm]  datetime2        NULL,
        CONSTRAINT [PK_Dependentes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Dependentes_Inquilinos_InquilinoId]
            FOREIGN KEY ([InquilinoId]) REFERENCES [Inquilinos] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_Dependentes_Cpf]      ON [Dependentes] ([Cpf]);
    CREATE INDEX        [IX_Dependentes_InquilinoId] ON [Dependentes] ([InquilinoId]);

    CREATE TABLE [Faturas] (
        [Id]                   uniqueidentifier NOT NULL,
        [MesReferencia]        nvarchar(7)      NOT NULL,
        [ValorAluguel]         decimal(18,2)    NOT NULL,
        [ValorAgua]            decimal(18,2)    NOT NULL DEFAULT 0,
        [ValorLuz]             decimal(18,2)    NOT NULL DEFAULT 0,
        [KwMesAnterior]        decimal(18,3)    NULL,
        [KwAtual]              decimal(18,3)    NULL,
        [KwhValor]             decimal(18,4)    NULL,
        [DataLimitePagamento]  date             NOT NULL,
        [DataPagamento]        date             NULL,
        [CodigoPix]            nvarchar(500)    NULL,
        [CobrancaAsaasId]      nvarchar(100)    NULL,
        [Status]               int              NOT NULL,
        [InquilinoId]          uniqueidentifier NOT NULL,
        [CriadoEm]             datetime2        NOT NULL,
        [AtualizadoEm]         datetime2        NULL,
        CONSTRAINT [PK_Faturas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Faturas_Inquilinos_InquilinoId]
            FOREIGN KEY ([InquilinoId]) REFERENCES [Inquilinos] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_Faturas_InquilinoId_MesReferencia]
        ON [Faturas] ([InquilinoId], [MesReferencia]);

    INSERT INTO [__EFMigrationsHistory] VALUES (N'20260429125932_MigracaoInicial', N'9.0.4');

    COMMIT;
    PRINT '   OK.';
END
ELSE
    PRINT '   Ignorado (ja aplicado).';

-- ============================================================
-- [2] AdicionarWalletIdAsaasNaConfiguracao
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429131226_AdicionarWalletIdAsaasNaConfiguracao')
BEGIN
    PRINT '>> Aplicando: AdicionarWalletIdAsaasNaConfiguracao...';

    BEGIN TRANSACTION;

    ALTER TABLE [Configuracoes]
        ADD [WalletIdAsaas] nvarchar(100) NULL;

    INSERT INTO [__EFMigrationsHistory] VALUES (N'20260429131226_AdicionarWalletIdAsaasNaConfiguracao', N'9.0.4');

    COMMIT;
    PRINT '   OK.';
END
ELSE
    PRINT '   Ignorado (ja aplicado).';

-- ============================================================
-- [3] AdicionarDataNascimentoInquilino
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260429220437_AdicionarDataNascimentoInquilino')
BEGIN
    PRINT '>> Aplicando: AdicionarDataNascimentoInquilino...';

    BEGIN TRANSACTION;

    -- Adiciona com default temporário (1900-01-01) para linhas existentes
    ALTER TABLE [Inquilinos]
        ADD [DataNascimento] date NOT NULL DEFAULT '1900-01-01';

    -- Remove o default permanente (EF não usa DEFAULT em colunas novas após criação)
    DECLARE @constraint_nascimento sysname;
    SELECT @constraint_nascimento = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID(N'[Inquilinos]') AND c.name = N'DataNascimento';

    IF @constraint_nascimento IS NOT NULL
        EXEC('ALTER TABLE [Inquilinos] DROP CONSTRAINT [' + @constraint_nascimento + ']');

    INSERT INTO [__EFMigrationsHistory] VALUES (N'20260429220437_AdicionarDataNascimentoInquilino', N'9.0.4');

    COMMIT;
    PRINT '   OK.';
END
ELSE
    PRINT '   Ignorado (ja aplicado).';

-- ============================================================
-- [4] AdicionarHostECamposMorador
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260430000404_AdicionarHostECamposMorador')
BEGIN
    PRINT '>> Aplicando: AdicionarHostECamposMorador...';

    BEGIN TRANSACTION;

    -- Novos campos em Inquilinos
    ALTER TABLE [Inquilinos]
        ADD [EstadoCivil]   int          NOT NULL DEFAULT 0,
            [OrgaoEmissor]  nvarchar(20) NOT NULL DEFAULT N'',
            [Rg]            nvarchar(20) NOT NULL DEFAULT N'',
            [Telefone]      nvarchar(20) NOT NULL DEFAULT N'';

    -- Nova tabela Hosts
    CREATE TABLE [Hosts] (
        [Id]                        uniqueidentifier NOT NULL,
        [NomeCompleto]              nvarchar(200)    NOT NULL,
        [Cpf]                       nchar(11)        NOT NULL,
        [DataNascimento]            date             NOT NULL,
        [Email]                     nvarchar(200)    NOT NULL,
        [SenhaHash]                 nvarchar(500)    NOT NULL,
        [EmailConfirmado]           bit              NOT NULL DEFAULT 0,
        [TokenConfirmacao]          nvarchar(100)    NULL,
        [TokenExpiracaoConfirmacao] datetime2        NULL,
        [CriadoEm]                  datetime2        NOT NULL,
        [AtualizadoEm]              datetime2        NULL,
        CONSTRAINT [PK_Hosts] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Hosts_Cpf]   ON [Hosts] ([Cpf]);
    CREATE UNIQUE INDEX [IX_Hosts_Email] ON [Hosts] ([Email]);
    CREATE UNIQUE INDEX [IX_Hosts_TokenConfirmacao]
        ON [Hosts] ([TokenConfirmacao])
        WHERE [TokenConfirmacao] IS NOT NULL;

    INSERT INTO [__EFMigrationsHistory] VALUES (N'20260430000404_AdicionarHostECamposMorador', N'9.0.4');

    COMMIT;
    PRINT '   OK.';
END
ELSE
    PRINT '   Ignorado (ja aplicado).';

-- ============================================================
-- [5] AdicionarValorGaragemEWhatsAppPix  ← NOVA (06/05/2026)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260506043939_AdicionarValorGaragemEWhatsAppPix')
BEGIN
    PRINT '>> Aplicando: AdicionarValorGaragemEWhatsAppPix...';

    BEGIN TRANSACTION;

    -- ValorGaragem em Faturas
    ALTER TABLE [Faturas]
        ADD [ValorGaragem] decimal(18,2) NOT NULL DEFAULT 0.00;

    -- Campos WhatsApp e PIX em Configuracoes
    ALTER TABLE [Configuracoes]
        ADD [ChavePix]               nvarchar(150)  NULL,
            [CidadeRecebedorPix]     nvarchar(15)   NULL,
            [MensagemPadraoWhatsapp] nvarchar(1000) NULL,
            [NomeRecebedorPix]       nvarchar(25)   NULL,
            [NumeroWhatsappLocador]  nvarchar(20)   NULL;

    INSERT INTO [__EFMigrationsHistory] VALUES (N'20260506043939_AdicionarValorGaragemEWhatsAppPix', N'9.0.4');

    COMMIT;
    PRINT '   OK.';
END
ELSE
    PRINT '   Ignorado (ja aplicado).';

-- ============================================================
-- Verificação final
-- ============================================================
PRINT '';
PRINT '=== Migrations aplicadas no banco: ===';
SELECT [MigrationId], [ProductVersion] FROM [__EFMigrationsHistory] ORDER BY [MigrationId];


