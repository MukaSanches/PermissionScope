# Continuous improvement ledger

## Rodada 2026-09-11 23:12 UTC — instrumentação por fase em validação

- PR #24: o head `a66ce57` teve Dependency Review, Windows Build, CodeQL e Production Trust aprovados. Como `main` avançou novamente, a branch foi sincronizada por merge sem force push no head `7920254`; documentação, recursos e site passaram localmente em oito idiomas e três larguras. O novo CI segue pendente; não integrar antes da aprovação completa.
- Implementado em lote separado `codex/worker-phase-metrics`: opção interna `--worker-metrics <arquivo>`, aceita somente com worker, registra scan, serialização única e escrita em sidecar atômico/best-effort. Sem a opção, stdout, snapshot e exit code permanecem iguais. Arquivos existentes não são sobrescritos; falha do sidecar não invalida o scan.
- `build/Measure-Scan.ps1 -MeasurePhases` registra leitura, parsing e validação do consumidor, além de memória do monitor antes/depois como observações de fronteira. Não chama esses valores de pico ou memória da GUI. Sidecar ausente/parcial é diagnóstico indisponível, nunca zero.
- Teste novo cobre protocolo sem flag, equivalência semântica, fases numéricas, falha de escrita, preservação de arquivo existente, argumentos inválidos e interrupção imediata. Build CLI passou sem warnings/erros; smoke 100 validou 201 objetos, zero erros/Unknown e diagnóstico completo. Um probe de 1 ms confirmou término forçado com diagnóstico ausente, sem métricas falsas. QA independente aprovou os três arquivos; ramos de sidecar corrompido e interrupção durante rename não foram injetados.
- Validação final: build Release x64 com o SDK .NET 10 isolado passou com zero warnings/erros; o harness Windows passou 88 testes e zero falhas; teste específico de métricas passou; verificador de documentação validou 65 documentos/72 imagens; `npm run check` e `git diff --check` passaram. A primeira tentativa de build usou o SDK global .NET 9 e foi rejeitada corretamente pelo `global.json`; a repetição usou a toolchain do projeto. O apphost de teste framework-dependent também exigiu `DOTNET_ROOT` apontado para a mesma toolchain e então passou; isso foi ambiente de teste, não falha do recurso.
- Medições smoke servem apenas para validar o instrumento e não são baseline. Um exemplo observado separou aproximadamente 300,02 ms de scan, 31,17 ms de serialização, 10,48 ms de escrita, 6,63 ms de leitura, 233,99 ms de parsing PowerShell e 36,84 ms de validação. Não representam GUI, SLA ou concorrentes.
- Próximo passo: concluir harness Windows e checks do lote; revisar diff; enviar PR separado. Depois repetir 10 mil com métricas por fase e decidir se Scanner ou transporte/consumidor domina o custo antes de qualquer mudança arquitetural ou teste de 100 mil.

### Autoanálise competitiva — 11/09/2026, rodada 23:12 UTC

A baseline de 10 mil e a nova instrumentação melhoram a capacidade de diagnosticar o próprio produto; não demonstram que ele ficou mais rápido nem que supera concorrentes. AccessChk continua mais amplo em tipos de objetos Windows; Netwrix e SolarWinds destacam usuários/grupos, AD e herança. PermissionScope mantém diferenciais documentados de evidência por ACE, Authz, Unknown explícito, execução local, snapshots e rollback, mas faltam testes equivalentes de correção, desempenho e usabilidade.

Distribuição v1.0.1, Apache-2.0, checksums, CI e documentação multilíngue são avanços públicos. Permanecem lacunas de assinatura Authenticode, Store/WinGet, ARM64 físico, acessibilidade WinUI certificada e ambientes reais de domínio/DFS. A prioridade discutida por produto, engenharia e QA é atribuir custo entre scanner, serialização e consumidor antes de otimizar ou ampliar o escopo.

Fontes oficiais consultadas nesta data: [Microsoft AccessChk](https://learn.microsoft.com/en-us/sysinternals/downloads/accesschk), [Netwrix Effective Permissions Reporting Tool](https://netwrix.com/en/resources/freeware/effective-permissions-reporting-tool/) e [SolarWinds Permissions Analyzer](https://www.solarwinds.com/free-tools/permissions-analyzer-for-active-directory).

## Rodada 2026-09-10 17:07 UTC — implementada e validada, integração adiada

- Apache-2.0 já implementada em `c79d101` e presente no PR #8; não repetir a migração. PR #8 ainda aberto no início desta rodada, head `c8e66fe`: CI x64 falhou, ARM64 e CodeQL passaram. A tarefa de publicação está corrigindo o fingerprint SVG (LF/CRLF) e preparando os pacotes.
- Coordenação confirmada: prioridade do usuário na outra tarefa é publicar a entrega existente, sem novas capturas/refactors. Não editar seu checkout, integrar este lote ou executar carga pesada durante a publicação.
- Trabalho isolado em `PermissionScope-benchmarks`, branch `codex/scan-scale-benchmark`, base `c8e66fe`. Escopo: harness reproduzível de benchmark, smoke pequeno e documentação. Nenhuma alteração no motor, app ou site.
- Próximo passo: após a entrega ativa, integrar o lote isolado `codex/scan-scale-benchmark`, confirmar a versão do CLI/pacote e executar os cenários de 10 mil/100 mil em ambiente controlado. Não repetir a licença nem considerar os resultados pequenos prova de escala.
- Equipe: engenharia implementou o harness; produto revisou concorrentes e prioridades; qualidade revisou segurança e semântica das medições. Produto e engenharia concordaram em priorizar CI/publicação e medir antes de mudar a arquitetura de scans.

### Relatório de implementação e validação

- Implementado: `build/Measure-Scan.ps1` com fixture exclusiva, limite de tempo, coleta de memória do CLI e saída em arquivos, validação do snapshot e teste separado de término forçado. Default de 100 arquivos; opt-in para mais de 10 mil. Nenhuma exclusão automática, alteração de ACL ou snapshot salvo na base do aplicativo.
- Documentado em `docs/benchmarks.md`: comandos, escopo, limites e cuidado com dados de identidade presentes nas saídas brutas. Apenas métricas sanitizadas aparecem neste relatório.
- QA encontrou e corrigiu falso positivo na execução que termina antes do pedido de cancelamento: agora exige exit 0 e snapshot válido/completo. Contagem de Unknown inclui decisões ausentes. Root e schema também são verificados.
- Validação final: parser PowerShell aprovado; smoke em NTFS com 100 arquivos e 101 diretórios (201 objetos), zero erros/Unknown, exit 0 e snapshot válido. Tempo 546,923 ms; pico observado CLI 40.783.872 bytes em sete amostras. Término forçado do processo: 26,2765 ms. Não mede GUI.
- O smoke inicial, antes das correções de QA, levou 13.006,3774 ms para os mesmos 201 objetos. A variabilidade e as tarefas concorrentes impedem tratar esses tempos como benchmark estável ou ganho de desempenho.
- `npm run check`, `node build/validate-documentation.mjs` (42 documentos, 72 imagens, oito idiomas) e `git diff --check` passaram. Sem build completo, novo harness Windows ou teste visual: nenhuma lógica do app/site foi alterada. Os cenários de timeout e conclusão antes do cancelamento foram revisados no código, mas não reproduzidos por testes controlados nesta rodada.
- Entrega: branch separada `codex/scan-scale-benchmark`, sem merge, release ou deploy deste lote. A publicação do PR #8 continua sob responsabilidade da tarefa ativa. A ausência de benchmark pesado é uma decisão de coordenação, não um resultado de performance.

### Autoanálise competitiva — 10/09/2026, segunda rodada

As fontes oficiais foram reabertas nesta rodada. Nenhuma mudança material nas capacidades anunciadas foi identificada.

| Dimensão | Comparação e evolução desde a rodada anterior |
| --- | --- |
| Recursos e cobertura | AccessChk também cobre Registro, serviços e processos. Netwrix/SolarWinds destacam usuários, grupos e herança. PermissionScope mantém foco em arquivos, evidências por ACE, snapshots e rollback; o harness não amplia cobertura funcional. |
| Correção e confiabilidade | Authz e Unknown preservam explicações e limites. Domínios/DFS ainda têm limitações. Não executamos corpus comparativo. A falha x64 do PR torna sua correção e validação do pacote prioridades imediatas. |
| Desempenho | Ainda não há medição comparativa nem scan 10k/100k concluído. O harness proposto mede CLI/worker, não memória total da GUI nem latência do botão de cancelar. Uma execução pequena serve para validar o instrumento, não para declarar escala. |
| UX e acessibilidade | Evolução proposta no PR #8: oito idiomas, 72 capturas e zero achados axe relatados em três larguras. São evidências do site/documentação; não certificam acessibilidade WinUI, traduções ou usabilidade comparativa. |
| Privacidade | Execução local, sem conta ou telemetria, permanece compromisso do projeto. Não houve auditoria comparativa de privacidade. Saídas brutas de benchmark podem conter caminhos e identidades do token local e devem permanecer locais. |
| Documentação, distribuição e manutenção | A migração Apache-2.0 está implementada no PR, com verificações de LICENSE/NOTICE. Documentação e entrega avançaram desde a rodada anterior, mas o PR estava aberto na consulta inicial. Publicação e aprovação de canais não podem ser presumidas. |

Síntese: o projeto avançou em apresentação e verificabilidade, mas segue sem evidência de superioridade corporativa ou de desempenho. A prioridade discutida entre produto e engenharia foi publicação verificável, depois medição real de escala; qualidade revisa o instrumento antes do uso. Nenhuma nota numérica foi atribuída por falta de critérios e testes equivalentes.

Fontes oficiais consultadas em 10/09/2026: [Microsoft AccessChk](https://learn.microsoft.com/en-us/sysinternals/downloads/accesschk), [Netwrix Effective Permissions Reporting Tool](https://netwrix.com/en/resources/freeware/effective-permissions-reporting-tool/) e [SolarWinds Permissions Analyzer](https://www.solarwinds.com/free-tools/permissions-analyzer-for-active-directory).

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

## Integrated batch checkpoint

Apache-2.0 commit `18cecf3` was integrated locally as `c79d101`, preserving the README/site generator work and LICENSE/NOTICE package verification. The independent UX review found and fixed missing Unknown guidance for unreadable resources and a stale demo filter. Documentation validation now passes 42 Markdown files and 72 captures. Windows integration: 88 passed. Website: eight locales, three viewport widths, zero axe violations. Official WinGet CLI manifest validation passed; submission is still pending published installer hashes.

Next action: push the combined documentation branch, review its CI, then integrate and prepare release assets. Keep Store account setup and unsigned-package limitations explicit. The screenshot fingerprint tracks application/engine sources, assets and project dependency declarations; test-only and license-only edits no longer invalidate rendered evidence.
