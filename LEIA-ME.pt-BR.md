# PermissionScope — começar

Use o instalador `PermissionScope-1.0.0-x64-Setup.exe` neste computador. A versão portátil também funciona: extraia o ZIP inteiro e abra `PermissionScope.exe`, mantendo todos os arquivos juntos.

## Primeira análise

1. Clique em **Analisar uma pasta** e escolha o local.
2. Deixe a conta vazia para verificar seu próprio acesso.
3. Clique em **Analisar** e selecione um item do resultado.
4. Em **Acesso**, veja as ações permitidas. Em **Access Path**, acompanhe as regras que explicam o resultado.
5. Salve um snapshot para comparar depois ou exporte um relatório.

**Indeterminado não significa negado.** Significa que faltam informações para uma conclusão segura. Permissões de rede e regras especiais exigem cuidados adicionais. As permissões analisadas também não garantem que um arquivo possa ser aberto: criptografia, bloqueios e outras políticas podem impedir isso.

Análises e simulações não modificam permissões. A opção separada de aplicar uma alteração modifica um arquivo local, exige confirmação digitada e mantém um registro para reversão.

## Se algo não funcionar

Leia a orientação exibida e abra **Detalhes técnicos**. O diagnóstico permanece em `%LOCALAPPDATA%\PermissionScope\last-operation-error.log`; revise caminhos e nomes antes de compartilhá-lo. Não é necessário enviar senhas.

O instalador ainda não tem assinatura comercial; o Windows pode mostrar um aviso de editor desconhecido. O pacote MSIX sem assinatura é um artefato de distribuição, não o instalador recomendado.

O código está sob licença Apache-2.0. Não há cobrança, assinatura, conta nem serviço próprio em nuvem. Consulte `docs/release-status.md` para os limites funcionais verificados.
