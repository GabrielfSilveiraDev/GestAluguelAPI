IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Apartamentos] (
    [Id] uniqueidentifier NOT NULL,
    [Numero] nvarchar(20) NOT NULL,
    [Bloco] nvarchar(10) NULL DEFAULT N'',
    [Ocupado] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_Apartamentos] PRIMARY KEY ([Id])
);

CREATE TABLE [Configuracoes] (
    [Id] uniqueidentifier NOT NULL,
    [KwhValor] decimal(18,4) NOT NULL,
    [ValorAgua] decimal(18,2) NOT NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_Configuracoes] PRIMARY KEY ([Id])
);

CREATE TABLE [GastosManutencao] (
    [Id] uniqueidentifier NOT NULL,
    [ApartamentoId] uniqueidentifier NOT NULL,
    [Descricao] nvarchar(500) NOT NULL,
    [Valor] decimal(18,2) NOT NULL,
    [Data] date NOT NULL,
    [Observacao] nvarchar(1000) NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_GastosManutencao] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GastosManutencao_Apartamentos_ApartamentoId] FOREIGN KEY ([ApartamentoId]) REFERENCES [Apartamentos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Inquilinos] (
    [Id] uniqueidentifier NOT NULL,
    [NomeCompleto] nvarchar(200) NOT NULL,
    [Cpf] nvarchar(11) NOT NULL,
    [QuantidadeMoradores] int NOT NULL,
    [DataEntrada] date NOT NULL,
    [DataVencimentoContrato] date NOT NULL,
    [ValorAluguel] decimal(18,2) NOT NULL,
    [Garagem] decimal(18,2) NOT NULL DEFAULT 0.0,
    [DiasAlertaVencimento] nvarchar(max) NOT NULL,
    [ApartamentoId] uniqueidentifier NOT NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_Inquilinos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Inquilinos_Apartamentos_ApartamentoId] FOREIGN KEY ([ApartamentoId]) REFERENCES [Apartamentos] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ContratosInquilino] (
    [Id] uniqueidentifier NOT NULL,
    [InquilinoId] uniqueidentifier NOT NULL,
    [NomeOriginalArquivo] nvarchar(260) NOT NULL,
    [CaminhoArquivo] nvarchar(500) NOT NULL,
    [TipoConteudo] nvarchar(100) NOT NULL,
    [TamanhoBytes] bigint NOT NULL,
    [Descricao] nvarchar(500) NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_ContratosInquilino] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContratosInquilino_Inquilinos_InquilinoId] FOREIGN KEY ([InquilinoId]) REFERENCES [Inquilinos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Dependentes] (
    [Id] uniqueidentifier NOT NULL,
    [NomeCompleto] nvarchar(200) NOT NULL,
    [Cpf] nvarchar(11) NOT NULL,
    [Rg] nvarchar(20) NULL,
    [OrgaoEmissor] nvarchar(20) NULL,
    [DataNascimento] date NOT NULL,
    [Telefone] nvarchar(20) NULL,
    [EstadoCivil] int NOT NULL,
    [InquilinoId] uniqueidentifier NOT NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_Dependentes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Dependentes_Inquilinos_InquilinoId] FOREIGN KEY ([InquilinoId]) REFERENCES [Inquilinos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Faturas] (
    [Id] uniqueidentifier NOT NULL,
    [MesReferencia] nvarchar(7) NOT NULL,
    [ValorAluguel] decimal(18,2) NOT NULL,
    [ValorAgua] decimal(18,2) NOT NULL DEFAULT 0.0,
    [ValorLuz] decimal(18,2) NOT NULL DEFAULT 0.0,
    [KwMesAnterior] decimal(18,3) NULL,
    [KwAtual] decimal(18,3) NULL,
    [KwhValor] decimal(18,4) NULL,
    [DataLimitePagamento] date NOT NULL,
    [DataPagamento] date NULL,
    [CodigoPix] nvarchar(500) NULL,
    [CobrancaAsaasId] nvarchar(100) NULL,
    [Status] int NOT NULL,
    [InquilinoId] uniqueidentifier NOT NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_Faturas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Faturas_Inquilinos_InquilinoId] FOREIGN KEY ([InquilinoId]) REFERENCES [Inquilinos] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AtualizadoEm', N'CriadoEm', N'KwhValor', N'ValorAgua') AND [object_id] = OBJECT_ID(N'[Configuracoes]'))
    SET IDENTITY_INSERT [Configuracoes] ON;
INSERT INTO [Configuracoes] ([Id], [AtualizadoEm], [CriadoEm], [KwhValor], [ValorAgua])
VALUES ('00000000-0000-0000-0000-000000000001', NULL, '2025-01-01T00:00:00.0000000Z', 0.0, 0.0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AtualizadoEm', N'CriadoEm', N'KwhValor', N'ValorAgua') AND [object_id] = OBJECT_ID(N'[Configuracoes]'))
    SET IDENTITY_INSERT [Configuracoes] OFF;

CREATE UNIQUE INDEX [IX_Apartamentos_Numero_Bloco] ON [Apartamentos] ([Numero], [Bloco]) WHERE [Bloco] IS NOT NULL;

CREATE INDEX [IX_ContratosInquilino_InquilinoId] ON [ContratosInquilino] ([InquilinoId]);

CREATE UNIQUE INDEX [IX_Dependentes_Cpf] ON [Dependentes] ([Cpf]);

CREATE INDEX [IX_Dependentes_InquilinoId] ON [Dependentes] ([InquilinoId]);

CREATE UNIQUE INDEX [IX_Faturas_InquilinoId_MesReferencia] ON [Faturas] ([InquilinoId], [MesReferencia]);

CREATE INDEX [IX_GastosManutencao_ApartamentoId] ON [GastosManutencao] ([ApartamentoId]);

CREATE INDEX [IX_Inquilinos_ApartamentoId] ON [Inquilinos] ([ApartamentoId]);

CREATE UNIQUE INDEX [IX_Inquilinos_Cpf] ON [Inquilinos] ([Cpf]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260429125932_MigracaoInicial', N'9.0.4');

ALTER TABLE [Configuracoes] ADD [WalletIdAsaas] nvarchar(100) NULL;

UPDATE [Configuracoes] SET [WalletIdAsaas] = NULL
WHERE [Id] = '00000000-0000-0000-0000-000000000001';
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260429131226_AdicionarWalletIdAsaasNaConfiguracao', N'9.0.4');

ALTER TABLE [Inquilinos] ADD [DataNascimento] date NOT NULL DEFAULT '0001-01-01';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260429220437_AdicionarDataNascimentoInquilino', N'9.0.4');

ALTER TABLE [Inquilinos] ADD [EstadoCivil] int NOT NULL DEFAULT 0;

ALTER TABLE [Inquilinos] ADD [OrgaoEmissor] nvarchar(20) NOT NULL DEFAULT N'';

ALTER TABLE [Inquilinos] ADD [Rg] nvarchar(20) NOT NULL DEFAULT N'';

ALTER TABLE [Inquilinos] ADD [Telefone] nvarchar(20) NOT NULL DEFAULT N'';

CREATE TABLE [Hosts] (
    [Id] uniqueidentifier NOT NULL,
    [NomeCompleto] nvarchar(200) NOT NULL,
    [Cpf] nchar(11) NOT NULL,
    [DataNascimento] date NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [SenhaHash] nvarchar(500) NOT NULL,
    [EmailConfirmado] bit NOT NULL DEFAULT CAST(0 AS bit),
    [TokenConfirmacao] nvarchar(100) NULL,
    [TokenExpiracaoConfirmacao] datetime2 NULL,
    [CriadoEm] datetime2 NOT NULL,
    [AtualizadoEm] datetime2 NULL,
    CONSTRAINT [PK_Hosts] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Hosts_Cpf] ON [Hosts] ([Cpf]);

CREATE UNIQUE INDEX [IX_Hosts_Email] ON [Hosts] ([Email]);

CREATE UNIQUE INDEX [IX_Hosts_TokenConfirmacao] ON [Hosts] ([TokenConfirmacao]) WHERE [TokenConfirmacao] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260430000404_AdicionarHostECamposMorador', N'9.0.4');

ALTER TABLE [Faturas] ADD [ValorGaragem] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Configuracoes] ADD [ChavePix] nvarchar(150) NULL;

ALTER TABLE [Configuracoes] ADD [CidadeRecebedorPix] nvarchar(15) NULL;

ALTER TABLE [Configuracoes] ADD [MensagemPadraoWhatsapp] nvarchar(1000) NULL;

ALTER TABLE [Configuracoes] ADD [NomeRecebedorPix] nvarchar(25) NULL;

ALTER TABLE [Configuracoes] ADD [NumeroWhatsappLocador] nvarchar(20) NULL;

UPDATE [Configuracoes] SET [ChavePix] = NULL, [CidadeRecebedorPix] = NULL, [MensagemPadraoWhatsapp] = NULL, [NomeRecebedorPix] = NULL, [NumeroWhatsappLocador] = NULL
WHERE [Id] = '00000000-0000-0000-0000-000000000001';
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260506043939_AdicionarValorGaragemEWhatsAppPix', N'9.0.4');

DROP INDEX [IX_Apartamentos_Numero_Bloco] ON [Apartamentos];

DELETE FROM [Configuracoes]
WHERE [Id] = '00000000-0000-0000-0000-000000000001';
SELECT @@ROWCOUNT;


ALTER TABLE [Inquilinos] ADD [HostId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Faturas] ADD [HostId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Configuracoes] ADD [HostId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Apartamentos] ADD [HostId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE UNIQUE INDEX [IX_Configuracoes_HostId] ON [Configuracoes] ([HostId]);

CREATE UNIQUE INDEX [IX_Apartamentos_HostId_Numero_Bloco] ON [Apartamentos] ([HostId], [Numero], [Bloco]) WHERE [Bloco] IS NOT NULL;

ALTER TABLE [Apartamentos] ADD CONSTRAINT [FK_Apartamentos_Hosts_HostId] FOREIGN KEY ([HostId]) REFERENCES [Hosts] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Configuracoes] ADD CONSTRAINT [FK_Configuracoes_Hosts_HostId] FOREIGN KEY ([HostId]) REFERENCES [Hosts] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260506145933_AdicionarHostIdMultiTenancy', N'9.0.4');

COMMIT;
GO

