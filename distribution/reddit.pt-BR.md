# Reddit: publicação por Samuel Sanches

Material preparado; nenhum post enviado. Publicar somente após confirmar que código e downloads estão públicos. Os links do produto abaixo são destinos previstos, não comprovação de publicação.

## Onde postar

| Comunidade | Conteúdo adequado | Antes de enviar |
|---|---|---|
| [r/SideProject](https://www.reddit.com/r/SideProject/) | Apresentação, imagem e pedido de feedback sobre clareza | Conferir [regras](https://www.reddit.com/r/SideProject/about/rules/) e elegibilidade |
| [r/opensource](https://www.reddit.com/r/opensource/) | Código Apache-2.0, modelo de acesso, corpus de testes | Conferir [regras](https://www.reddit.com/r/opensource/about/rules/) e flair promocional quando exigido |
| [r/sysadmin](https://www.reddit.com/r/sysadmin/) | Caso técnico reproduzível no espaço permitido para projetos/fornecedores | Conferir [regras](https://www.reddit.com/r/sysadmin/about/rules/) e tópico vigente; não anunciar indiscriminadamente |
| [r/github](https://www.reddit.com/r/github/) | Resumo com link do código | Usar o megathread vigente, seguindo a [orientação dos moderadores](https://www.reddit.com/r/github/comments/1jy8rea/promote_your_projects_here_selfpromotion/) |

Consulta em 10/09/2026: as páginas oficiais de regras retornaram conteúdo incompleto sem autenticação. A permissão para postar deve ser confirmada na conta do autor. Não há horário ideal ou viralização comprovada para o produto.

## Post em inglês

**Title:** I built PermissionScope to explain Windows file permissions — free, local and open source

I'm Samuel Sanches, the creator of PermissionScope.

It is a Windows desktop app for checking file and folder permissions and inspecting the rules behind a result. The Access Path view connects the selected identity to the applicable permission entries and available group evidence.

The engine uses Windows Authz. You can save scans, compare observations, remove an entry in a simulation, and export reports. There is also a CLI. Analysis and simulation do not change permissions.

It runs locally, requires no account and has no telemetry or paid tier. The source is Apache-2.0 licensed.

There are limits: a remote server's actual logon context is not verified, some conditional policies need information the app cannot establish, and inherited entries do not yet identify the exact originating ancestor. Results remain Unknown or explicitly scoped. The direct downloads are currently unsigned.

Source and limitations: https://github.com/MukaSanches/PermissionScope

Downloads: https://github.com/MukaSanches/PermissionScope/releases

I'd appreciate feedback on whether the explanation is understandable without already knowing Windows ACL terminology. If you find a calculation issue, a small synthetic SDDL example would help me reproduce it without exposing your environment.

## Variante técnica

**Title:** PermissionScope: Windows Authz results with inspectable ACL evidence (Apache-2.0)

Use a apresentação acima, substituindo o pedido final por:

The repository includes executable cases for NULL versus empty DACLs, stored ACE order, deny-only groups and Owner Rights, alongside Windows integration checks. I'd welcome minimal examples where the displayed evidence fails to explain the Authz result. This is discretionary analysis, not proof that a file-open operation will succeed.

Access model: https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md

## Post em português

**Título:** Criei o PermissionScope para explicar permissões do Windows — gratuito, local e de código aberto

Sou Samuel Sanches, criador do PermissionScope. Ele ajuda a verificar permissões de arquivos e pastas e entender quais regras sustentam o resultado.

O cálculo usa o Windows Authz. A interface mostra as entradas de permissão e as evidências disponíveis de grupos. É possível salvar análises, comparar observações, simular a remoção de uma regra e exportar relatórios. Também há uma CLI.

Não exige conta, não tem telemetria nem versão paga. Analisar e simular não altera as permissões. O código é Apache-2.0.

Há limites documentados: o programa não comprova o contexto real de logon de um servidor remoto e não resolve todas as políticas condicionais. Quando faltam informações, mostra Unknown. Os instaladores diretos ainda não têm assinatura digital.

Código e downloads: https://github.com/MukaSanches/PermissionScope

Quero ouvir principalmente se a explicação fica clara para quem não conhece ACLs. Exemplos sintéticos de resultados incorretos também são muito úteis.

## Imagens

1. `media/01-home-pt-BR.png`: captura real existente da tela inicial. Legenda: “PermissionScope — análise local de permissões do Windows”. Alt: “Tela inicial em português, com opções para analisar pasta, pesquisar pessoa e abrir análise anterior”.
2. `media/02-access-path.png`: diagrama explicativo, identificado como exemplo sintético; não é captura da interface. Legenda: “Identidade, grupo, regra, recurso e resultado: como ler a evidência”. Alt: “Fluxo ilustrativo de evidência de uma permissão de leitura; contexto ausente permanece Unknown”.

Não usar `artifacts/analysis.png`: contém nomes de máquina, perfil e caminhos locais. Não apresentar o diagrama como prova de cobertura completa de domínios ou de herança.

## Respostas curtas

- **É grátis?** Sim. Não há versão paga, conta obrigatória ou limite comercial. O código é Apache-2.0; a política Free Forever descreve o compromisso do projeto.
- **Por que Unknown?** Porque o contexto disponível não sustenta uma conclusão confiável. A evidência e a limitação ficam visíveis.
- **É seguro?** Código, modelo de acesso e testes são inspecionáveis. Análise é somente leitura; mudanças reais são separadas. Os binários diretos ainda são unsigned; comece em um ambiente de teste.
- **Foi feito com IA?** Houve assistência de IA no desenvolvimento e na revisão. Samuel Sanches é o criador e mantenedor. As conclusões devem ser sustentadas pelo Windows e pelos testes, não por essa assistência.
- **Supera todas as alternativas?** Não há benchmark abrangente que sustente essa afirmação. O foco é evidência inspecionável, operação local e gratuidade.

Contato público: ssanches011@gmail.com. Não inventar clientes, depoimentos, votos ou quantidade de profissionais envolvidos.
