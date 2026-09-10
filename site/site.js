"use strict";
const portuguese = {
  skip:"Ir para o conteúdo",navEvidence:"Evidências",navDownload:"Baixar",language:"Idioma",eyebrow:"Inteligência de acesso Windows",
  hero:"Veja quem tem acesso.\nEntenda o motivo.",intro:"Leia as permissões. Acompanhe as evidências. Saiba o que mudou — sem enviar seu ambiente para um serviço em nuvem.",
  download:"Baixar PermissionScope",model:"Entenda o modelo de acesso →",promise:"Gratuito para sempre · Local · Sem conta",caption:"O aplicativo Windows real. Sem interface web embutida. Sem resultados inventados.",
  calculated:"Calculado. Sem adivinhação.",evidenceTitle:"Um resultado que você pode questionar.",evidenceIntro:"Uma resposta sobre permissões só é útil quando você consegue conferir o que a sustenta. O Access Path mantém essa evidência junto do resultado.",
  whoTitle:"Quem",who:"Veja as contas e os grupos nas regras de acesso de uma pasta. Avalie uma identidade Windows por vez — sem inventar uma contagem de todas as pessoas que poderiam ter acesso.",
  whyTitle:"Por quê",why:"Acompanhe as regras que contribuem para o acesso, a participação no token e as relações de grupo comprovadas. Abra o descritor original quando precisar auditar os dados técnicos.",
  changeTitle:"O que mudou",change:"Salve observações locais e compare descritores e acesso avaliado. Um recurso ausente em uma análise posterior não é considerado excluído automaticamente.",
  simulationTitle:"E se",simulation:"Remova uma regra em memória e recalcule. Aplicar a mudança é uma ação separada: apenas arquivos locais, confirmação explícita, registro e reversão verificada.",
  trustLabel:"Confiança técnica",unknownTitle:"“Indeterminado” é uma resposta honesta.",unknown:"O Windows Authz calcula o acesso discricionário. Um contexto remoto, uma regra condicional ou uma origem de herança não comprovada não são preenchidos com suposições.",limits:"Uma permissão não garante que o arquivo possa ser aberto. Criptografia, bloqueios, políticas de integridade e outras restrições ainda podem impedir o acesso. Os resultados remotos são deliberadamente conservadores.",scope:"Consulte o escopo suportado →",
  downloadLabel:"PermissionScope para Windows",downloadTitle:"Suas permissões.\nSeu computador.",requirements:"Windows 10 1809 ou posterior. WinUI 3 nativo. Os pacotes completos incluem os runtimes do .NET e do Windows App SDK.",
  x64:"Instalador Windows x64",x64Hint:"Maioria dos computadores Intel e AMD",portable:"ZIP portátil x64",portableHint:"Extraia a pasta inteira; não exige instalação",arm:"Instalador Windows ARM64",armHint:"Compilação cruzada; não executado em um dispositivo ARM nos testes",unsigned:"Os instaladores ainda não têm assinatura digital. O Windows pode avisar que o editor é desconhecido. Confira os hashes da versão antes de usar.",release:"Notas da versão e hashes",source:"Código-fonte",
  privacyTitle:"Privacidade faz parte da arquitetura.",privacy:"Sem conta, anúncios, telemetria do aplicativo ou versão paga. As análises ficam no seu computador. O Windows contata apenas os recursos de rede necessários à análise solicitada. Revise os relatórios antes de compartilhar: caminhos e nomes de contas podem ser sensíveis.",hosting:"Este site não usa analytics nem fontes externas. O GitHub, como provedor de hospedagem e downloads, processa solicitações web normais conforme suas próprias políticas.",privacyLink:"Detalhes de privacidade →",footer:"Gratuito para sempre. Código aberto sob MIT.",security:"Relatar uma vulnerabilidade"
};
const elements = [...document.querySelectorAll("[data-text]")];
const english = new Map(elements.map(element => [element.dataset.text, element.cloneNode(true)]));
const selector = document.getElementById("language");
function setLanguage(language) {
  const isPortuguese = language === "pt-BR";
  document.documentElement.lang = isPortuguese ? "pt-BR" : "en";
  selector.value = isPortuguese ? "pt-BR" : "en";
  for (const element of elements) {
    const translated = portuguese[element.dataset.text];
    if (isPortuguese && translated) {
      element.replaceChildren(...translated.split("\n").flatMap((line, index) => index ? [document.createElement("br"), document.createTextNode(line)] : [document.createTextNode(line)]));
    } else { element.replaceChildren(...[...english.get(element.dataset.text).childNodes].map(node => node.cloneNode(true))); }
  }
}
selector.addEventListener("change", () => {
  setLanguage(selector.value);
  const url = new URL(location.href);
  url.searchParams.set("lang", selector.value);
  history.replaceState(null, "", url);
});
setLanguage(new URL(location.href).searchParams.get("lang") ?? (navigator.language.startsWith("pt") ? "pt-BR" : "en"));
