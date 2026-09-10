# Lançamento e manutenção

Samuel Sanches · ssanches011@gmail.com. Objetivo: adoção útil e correção verificável. Viralização não é garantida.

## Sequência

1. Confirmar código, download, checksums e workflows públicos. Publicar versão definitiva só depois das verificações necessárias.
2. Demonstrar um problema concreto com dados sintéticos: explicar por que uma identidade tem determinada permissão. Abrir espaço para feedback no GitHub.
3. Apresentar em uma comunidade compatível por vez, conforme suas regras. Responder e corrigir problemas antes de ampliar divulgação. Usar `reddit.pt-BR.md`.
4. Cadastrar no AlternativeTo e Product Hunt quando houver download funcional público. Usar imagens reais e o escopo documentado, sem promessas de cobertura completa.
5. Divulgar comandos de Store/WinGet/Chocolatey apenas após aceitação real dos canais. Até lá, usar downloads do projeto.

## Conteúdo útil para compartilhar

| Tema | Entrega | Prova |
|---|---|---|
| DACL vazia versus ausente | Explicação curta com exemplo | Resultado Authz e fixture do corpus |
| Ordem de allow e deny | Máscara afetada e ACE específica | Caso não canônico reproduzível |
| Grupo habilitado versus deny-only | Contexto do token | Teste e referência Windows |
| O que mudou entre análises | Comparação de descritores | Não atribuir autoria que o snapshot não registra |
| Por que Unknown é útil | Contexto remoto não estabelecido | Limitação explícita, sem certeza inventada |

Cada texto deve resolver uma pergunta e apontar para um teste ou trecho relevante. Evitar artigos genéricos em volume. Relatórios já identificam o PermissionScope; o compartilhamento continua sob controle de quem exporta, após revisar os dados.

## MSPs e equipes pequenas

Oferecer uma receita de avaliação: instalar em laboratório, analisar uma pasta sintética, exportar HTML, comparar uma alteração controlada e verificar os descritores. Sem cadastro ou licença por cliente. Convidar voluntários a enviar fixtures sanitizadas pelo GitHub; não coletar inventários de clientes nem enviar mensagens privadas em massa.

## Sustentabilidade

- Cada correção de cálculo recebe regressão reproduzível no corpus.
- Dependabot, CI e CodeQL reduzem trabalho manual; alertas continuam exigindo análise. Não automatizar aprovação de código ou mudanças de ACL.
- Releases pequenas e imutáveis, escopo explícito e rollback documentado. Não prometer suporte empresarial 24 horas.
- Agrupar dúvidas recorrentes em documentação. Priorizar cálculo incorreto, perda de dados e mudanças inesperadas.
- Traduzir por demanda e disponibilidade de revisores; não chamar tradução automática de revisão humana.
- Manter site estático, sem rastreamento ou serviços pagos obrigatórios.

## Medição sem telemetria

Revisar downloads por release (não equivalem a usuários), issues reproduzíveis, tempo até correção e contribuições aceitas. Registrar fonte e data. Não usar estrelas como prova de qualidade, comprar engajamento ou coletar dados no aplicativo.

Na primeira semana, concentrar o trabalho em instalação e explicações confusas. Depois, publicar casos resolvidos e melhorias concretas. Ampliar canais quando a instalação estiver estável e houver capacidade para responder.
