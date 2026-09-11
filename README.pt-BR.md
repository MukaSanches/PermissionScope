<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>Permissões do Windows, finalmente inspecionáveis.</strong><br>Entenda quem pode acessar uma pasta e confira as regras que sustentam a resposta.</p>
<table><tr><td align="center"><a href="https://mukasanches.github.io/PermissionScope/index.pt-BR.html"><strong>Site</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/releases/latest"><strong>Downloads da versão</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md"><strong>Central de confiança</strong></a></td></tr><tr><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">Academia</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">Roteiro</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Segurança</a></td></tr></table>

<p align="center"><sub>Idiomas</sub></p>
<table><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.md"><strong>English</strong></a><br><sub>en-US</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md"><strong>Português</strong></a><br><sub>pt-BR</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md"><strong>Español</strong></a><br><sub>es</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md"><strong>Français</strong></a><br><sub>fr</sub></td></tr><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md"><strong>Deutsch</strong></a><br><sub>de</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md"><strong>العربية</strong></a><br><sub>ar</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md"><strong>日本語</strong></a><br><sub>ja</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md"><strong>简体中文</strong></a><br><sub>zh-Hans</sub></td></tr></table>

---

<table><tr><td width="33%"><strong>Local-first</strong><br><sub>Sem conta PermissionScope, telemetria do aplicativo ou serviço de nuvem obrigatório.</sub></td><td width="33%"><strong>Decisão nativa do Windows</strong><br><sub>O acesso efetivo é avaliado com Windows Authz, não por uma aproximação criada à mão.</sub></td><td width="33%"><strong>Desconhecido continua Desconhecido</strong><br><sub>Contexto ausente nunca é transformado silenciosamente em Permitido ou Negado.</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/pt-BR/access-light.png" alt="LAB\Alex recebe Modificar por LAB\Finance. As capturas são do aplicativo nativo com uma demonstração sintética avaliada por Authz, sem dados de uma empresa real nem resultados alterados." width="94%"></p>

## Comece em dois minutos

1. Baixe o instalador completo ou ZIP portátil em Releases. Escolha x64 para Intel/AMD ou ARM64 para um PC ARM.
2. Instale para sua conta ou extraia o ZIP inteiro e abra PermissionScope.exe.
3. Escolha Explorar demonstração para aprender com LAB\Alex. Para seus arquivos, escolha Analisar e informe uma pasta.
4. Deixe a identidade vazia para usar seu token Windows atual. Abra Access Path para conferir as evidências.

## Do resultado à evidência

O Windows Authz calcula a máscara. Access Path mostra as entradas que contribuem e as associações registradas. O sinalizador de herança não comprova a pasta de origem. Controle total na ACL discricionária não garante que um arquivo possa ser aberto.

```text
Identidade do Windows
      ↓
SID + contexto de associação registrado
      ↓
ACL / entradas de permissão discricionária
      ↓
Avaliação pelo Windows Authz
      ↓
Permitido · Parcial · Negado · Desconhecido
      ↓
Access Path → evidências contribuintes
```

## Entenda o resultado

Permitido autoriza a ação indicada pelas regras discricionárias avaliadas. Parcial permite apenas algumas ações. Negado significa que essas regras não concedem acesso. Desconhecido indica contexto insuficiente para confirmar a resposta; nunca é tratado como permitido.

## Confira a explicação

O Windows Authz calcula a máscara. Access Path mostra as entradas que contribuem e as associações registradas. O sinalizador de herança não comprova a pasta de origem. Controle total na ACL discricionária não garante que um arquivo possa ser aberto.

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/pt-BR/access-path-light.png" alt="Access Path" width="94%"></p>

## Superfície do produto

<table><tr><td><strong>Analisar</strong><br><sub>Inspecione o acesso efetivo de uma pasta e identidade.</sub></td><td><strong>Explicar</strong><br><sub>Siga o Access Path e as evidências que contribuíram.</sub></td><td><strong>Snapshot</strong><br><sub>Salve observações locais para revisar depois.</sub></td></tr><tr><td><strong>Comparar</strong><br><sub>Compare observações sem presumir que recursos ausentes foram excluídos.</sub></td><td><strong>Simular</strong><br><sub>Modele a remoção de uma regra em memória antes de considerar uma alteração real.</sub></td><td><strong>Exportar</strong><br><sub>Saídas HTML, CSV, JSON, XLSX e PDF.</sub></td></tr></table>

## Preserve as evidências

Salve snapshots locais, compare observações, simule a remoção de uma regra em memória e exporte HTML, CSV, JSON, XLSX ou PDF. Um recurso ausente na observação posterior não é considerado excluído automaticamente.

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## Escopo e privacidade

Logons remotos, contextos S4U, regras condicionais e destinos de links não verificados permanecem Desconhecidos. Integridade, criptografia, bloqueios e privilégios administrativos ficam fora dessa decisão. Sem telemetria, conta ou serviço em nuvem. Caminhos e nomes em exportações reais podem ser sensíveis.

> Análise e simulação apenas leem. Aplicar é um fluxo separado, confirmado explicitamente, para arquivos locais comuns, com observação salva, registro durável, verificação e reversão. Pastas, links e arquivos com vários hard links são excluídos.

## Baixe e confira

Windows 10 1809 ou posterior. Pacotes completos incluem os runtimes. Instaladores ainda sem assinatura digital; confira SHA-256 com os hashes da release. ARM64 é compilado no CI, sem certificação em hardware ARM. Consulte o status para disponibilidade na Store e no WinGet.

[Downloads da versão](https://github.com/MukaSanches/PermissionScope/releases/latest) · [Checksums SHA-256](https://github.com/MukaSanches/PermissionScope/releases/latest/download/SHA256SUMS.txt) · [Status da versão](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## Construído para ser verificado

| Construído para ser verificado | |
|---|---|
| Código-fonte | Repositório público e histórico de commits |
| Releases | Artefatos versionados e checksums SHA-256 |
| Cadeia de fornecimento | SBOM CycloneDX e automações fixadas quando documentado |
| Segurança | Modelo de segurança, divulgação responsável e CodeQL |
| Plataforma | Builds x64 e ARM64 |
| Documentação | Oito handbooks localizados, guias visuais e cursos da Academy |
| Privacidade | Arquitetura local-first e orientação explícita sobre dados sensíveis em exports |

[Abrir o Trust Center](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## Aprenda o modelo, não só os botões

- [Fundamentos de permissões](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [Lendo o Access Path](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [Diagnóstico seguro](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [Manuais em PDF · 8 idiomas](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## Escolha o próximo passo

- [Guia visual](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/pt-BR.md)
- [Modelo técnico](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [Perguntas e solução de problemas](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [Glossário em linguagem simples](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [Privacidade](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [Status da versão](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [Governança](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [Suporte](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [Citação](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## Limites de engenharia

O PermissionScope não finge que a ACL discricionária explica todo resultado possível ao abrir arquivos. Integridade, criptografia, bloqueios, alguns contextos remotos/S4U, regras condicionais, privilégios administrativos e destinos de reparse não verificados podem ficar fora do contexto disponível. Esses limites são documentados, não escondidos.

## Compile e teste

Use Windows e o SDK .NET 10. Os testes usam um executável de integração, não dotnet test. Veja o guia de desenvolvimento para empacotamento e verificação documental.

[Desenvolvimento](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [Desempenho](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [Roteiro](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## Ajuda e contribuição

Informe versão, Windows, operação e código de erro. A aba Técnica copia um diagnóstico sem caminhos, contas ou SIDs. Traduções são prévias; evidências técnicas podem permanecer em inglês. Não alegamos revisão por falantes nativos nem certificação assistiva.

---

<p align="center"><sub>Criado por Samuel Sanches · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
