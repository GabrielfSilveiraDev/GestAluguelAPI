
---

## Resumo das tarefas do frontend derivadas dessas mudanças

1. **Criar página `/login`** com abas Administrador e Inquilino.
2. **Interceptor de requisições:** incluir `Authorization: Bearer {token}` e tratar 401.
3. **Adicionar campo `dataNascimento`** (obrigatório) no formulário de criação de inquilino.
4. **Campo `bloco` não obrigatório** nos formulários de apartamento.
5. **Criar rota `/portal`** com as três seções para o inquilino.
6. **Roteamento por perfil:** após login, redirecionar `Host` → `/` (dashboard) e `Inquilino` → `/portal`.
