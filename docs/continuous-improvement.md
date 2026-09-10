# Continuous improvement ledger

## Current checkpoint — 2026-09-10

Working branch: `codex/documentation-and-explainability`. Base main: `6335eed`. Repository: [MukaSanches/PermissionScope](https://github.com/MukaSanches/PermissionScope).

Delivered locally in this batch:

- Synthetic LAB fixture evaluated through Windows Authz, with deterministic snapshots, five export formats and an after-snapshot. No accounts or resource ACLs are created.
- Plain-language summaries, explicit Unknown explanation, safe diagnostics, demo entry point and guarded simulation. CLI simulation returns complete decisions and Unknown exit status.
- Eight equivalent generated READMEs, localized walkthroughs and static website pages; 72 native light/dark captures with source fingerprint, hash, locale and privacy checks.
- Installation, CLI, FAQ, glossary, accessibility, development and documentation-maintenance guides. Eight draft Store listings, pending the user's actual Partner Center identity.
- Pinned GitHub Actions to upstream commit SHAs. Documentation verification added to CI and release checks.

Evidence: 88 Windows integration tests passed; eight website locales passed automated axe checks at 375/768/1440 widths; documentation validator passed 41 documents and 72 capture hashes. These are local results for this batch. Native screen-reader certification, physical ARM testing and worldwide translation review are not claimed.

## Next steps

1. Incorporate independent review findings, inspect the final diff, commit the batch and open a PR. Keep capture/source synchronization if code changes.
2. Confirm PR CI, then merge. Rebuild final packages and verify checksums, source archive and release manifest before publishing the release.
3. Validate official WinGet manifests against the published installers and submit through the official repository. Do not claim channel availability before acceptance.
4. Resume Store only after the real Partner Center product/publisher identity exists. The account/identity step belongs to the user; no password or verification code belongs in this repository.

## Operating rules for subsequent cycles

Use bounded, evidence-driven improvements. Preserve active work; never force-push or overwrite unrelated edits. Review performance, correctness, UX and distribution separately, then prioritize a concrete issue with a test or measurable outcome. Use official sources for current technical facts. Keep paid services and usage-reset purchases outside the automation. Publish no social posts without explicit authorization for the message.

Update this checkpoint before long operations and after completed milestones. If usage or authentication prevents progress, retain the exact next step and report the blocker once. A scheduled run depends on account quota and the host being available.
## Independent licensing and competitive review

## Rodada de 2026-09-10 — implementação e validação concluídas; integração pendente

Prioridade solicitada: migrar o código próprio para Apache-2.0 e estabelecer uma comparação fundamentada com concorrentes.

- Base: `6335eed`; branch isolada `codex/apache-2-license` no worktree `PermissionScope-continuous`.
- O checkout `PermissionScope` contém trabalho ativo de documentação e UX em `codex/documentation-and-explainability`. Nenhuma alteração desse checkout deve ser descartada ou incluída automaticamente neste lote.
- Coordenação: a tarefa de documentação atualizará README, gerador, documentos e site gerados. Esta rodada altera a licença e os arquivos-base restantes. Não integrar nem publicar release até concluir a coordenação.
- Histórico Git examinado: os três commits da base identificam Samuel Sanches, também titular no LICENSE anterior; não foi identificado outro autor no histórico examinado. Licenças de dependências permanecem próprias.
- Equipe: revisão de engenharia, avaliação de produto/concorrentes e auditoria de licença, sem edição concorrente.
- Próximo passo: entregar esta branch à tarefa de documentação para cherry-pick e integração com as referências geradas; verificar CI, licença em todos os idiomas e pacotes antes de publicação. Não repetir a migração já implementada neste lote.

Fontes da migração: https://www.apache.org/licenses/LICENSE-2.0.html e https://www.apache.org/licenses/LICENSE-2.0.txt (consultadas em 2026-09-10).

### Relatório da rodada

Implementado: LICENSE integral oficial, NOTICE com autoria de Samuel Sanches, Apache-2.0 nos metadados MSBuild e npm raiz, política gratuita, contribuição, LEIA-ME, corpus de testes, rascunhos de divulgação e gerador WinGet. O empacotamento passa a copiar NOTICE e declarar a licença do aplicativo no SBOM. As licenças das dependências foram preservadas. CHANGELOG registra a mudança; binários anteriores não foram substituídos.

Validado: igualdade exata de LICENSE com novo download oficial; `npm run check` aprovado (oito catálogos, textos obrigatórios, placeholders, assets, landmarks e sintaxe JS); `git diff --check` aprovado; parsing dos dois scripts PowerShell alterados aprovado; XML de MSBuild com Apache-2.0 verificado; revisão do diff de terceiros e metadados npm. Esta rodada não executou build completo, harness de integração Windows, teste visual nem reconstrução de instaladores: não houve alteração de lógica do aplicativo e o lote combinado permanece sob validação da tarefa ativa. Os resultados não comprovam um release novo.

GitHub examinado: base `6335eed` com Windows build aprovado; consultas também mostraram Windows build e CodeQL aprovados para `90ddef6`. PRs abertos #6 (axe-core) e #7 (GitHub Actions) são atualizações de dependências, não foram integrados nesta rodada. O executável gh não estava no PATH; foi localizado na toolchain existente e usado sem instalação adicional.

Publicação: entrega preparada na branch `codex/apache-2-license`, destinada a cherry-pick na tarefa `codex/documentation-and-explainability`. Sem merge, release ou deploy nesta rodada. O README, o site e o gerador permanecem sob responsabilidade da tarefa ativa, que confirmou atualização para Apache-2.0. A verificação de LICENSE/NOTICE em pacotes foi solicitada ao responsável por Verify-Release.ps1. O próximo ciclo deve conferir o resultado real, não presumir conclusão da integração.

### Autoanálise competitiva — 10/09/2026

Avaliação da base versionada desta rodada, sem incorporar como concluídas as alterações ainda em andamento no outro checkout. Esta é a primeira linha de base; não há rodada anterior comparável.

| Dimensão | Evidência e avaliação |
| --- | --- |
| Recursos e cobertura | PermissionScope documenta GUI/CLI, evidências por ACE, snapshots, comparação, cinco formatos de relatório e rollback. AccessChk também cobre Registro, serviços, processos e objetos, além de arquivos/diretórios: possui escopo de objetos mais amplo. |
| Correção e domínio | Uso de Authz e estados Unknown tornam limites explícitos. Netwrix e SolarWinds anunciam análise de grupos, usuários e herança. Não executamos corpus equivalente nessas ferramentas; não há conclusão de superioridade em correção. O projeto declara cobertura incompleta de DFS, trusts e continuação de membros LDAP. |
| Confiabilidade | Confirmação e rollback são pontos positivos do desenho. Cancelar o worker perde observações incompletas; não foi validada uma implantação corporativa representativa. |
| Desempenho | Não comparado. Scanner acumula resultados, CLI serializa snapshot inteiro numa linha e GUI desserializa outra cópia. Pico alto de memória é hipótese fundamentada no código, ainda sem medição. O benchmark de scan publicado tem só três diretórios; testes sintéticos de 100 mil objetos não equivalem a scan real. |
| UX e acessibilidade | Interface nativa e PT-BR/inglês são recursos documentados. Usabilidade frente a concorrentes, leitor de tela e alto contraste não foram avaliados nesta rodada; não há certificação estabelecida. |
| Privacidade | O projeto declara execução local, sem conta ou telemetria. As fontes consultadas não sustentam alegação de superioridade sobre os concorrentes nessa dimensão. |
| Documentação e distribuição | Limites técnicos estão descritos. Binários sem assinatura e ausência de submissão Store/WinGet permanecem lacunas. Adoção, suporte e cadência dos concorrentes não foram medidos. |

Síntese inferida: ferramenta especializada em explicabilidade local, com maturidade corporativa ainda limitada. Não há evidência suficiente de que supera os concorrentes. A migração Apache-2.0 esclarece os termos de reutilização, mas não melhora por si só o motor, a acessibilidade ou o desempenho.

Fontes oficiais consultadas em 10/09/2026:

- Microsoft AccessChk: https://learn.microsoft.com/en-us/sysinternals/downloads/accesschk
- Netwrix Effective Permissions Reporting Tool: https://netwrix.com/en/resources/freeware/effective-permissions-reporting-tool/
- SolarWinds Permissions Analyzer: https://www.solarwinds.com/free-tools/permissions-analyzer-for-active-directory

### Decisão da equipe e próximos passos

Produto propôs priorizar escala e cancelamento; engenharia concordou e recomendou medir antes de implementar streaming. Evidências: Scanner.cs acumula a lista, CLI Program.cs serializa o snapshot, ScanWorker.cs recebe/desserializa e encerra o processo no cancelamento; docs/release-status.md reconhece a limitação. A auditoria de licença confirmou as referências próprias e exigiu preservar os avisos de terceiros. Não houve divergência material; o lote ficou restrito à migração solicitada para evitar colisão com documentação/UX em andamento.

1. Confirmar cherry-pick, referências geradas Apache-2.0, verificação de LICENSE/NOTICE e CI do lote combinado. Manter o ponto de retomada atualizado.
2. Medir scan NTFS de 10 mil/100 mil objetos sintéticos: tempo, pico de memória de CLI e GUI, latência de cancelamento e perda de progresso. Usar fixtures isoladas e limites de execução definidos.
3. Usar os resultados para decidir persistência incremental e recuperação em lote separado; ampliar depois a validação de domínio e acessibilidade.
