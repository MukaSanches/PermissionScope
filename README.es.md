<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>Permisos de Windows, ahora inspeccionables.</strong><br>Entienda quién puede acceder a una carpeta y compruebe las reglas que explican el resultado.</p>
<table><tr><td align="center"><a href="https://mukasanches.github.io/PermissionScope/index.es.html"><strong>Sitio web</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/releases/latest"><strong>Descargas de la versión</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md"><strong>Centro de confianza</strong></a></td></tr><tr><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">Academia</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">Hoja de ruta</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Seguridad</a></td></tr></table>

<p align="center"><sub>Idiomas</sub></p>
<table><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.md"><strong>English</strong></a><br><sub>en-US</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md"><strong>Português</strong></a><br><sub>pt-BR</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md"><strong>Español</strong></a><br><sub>es</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md"><strong>Français</strong></a><br><sub>fr</sub></td></tr><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md"><strong>Deutsch</strong></a><br><sub>de</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md"><strong>العربية</strong></a><br><sub>ar</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md"><strong>日本語</strong></a><br><sub>ja</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md"><strong>简体中文</strong></a><br><sub>zh-Hans</sub></td></tr></table>

---

<table><tr><td width="33%"><strong>Local-first</strong><br><sub>Sin cuenta PermissionScope, telemetría de la aplicación ni servicio cloud obligatorio.</sub></td><td width="33%"><strong>Decisión nativa de Windows</strong><br><sub>El acceso efectivo se evalúa con Windows Authz y no con una aproximación manual.</sub></td><td width="33%"><strong>Desconocido sigue siendo Desconocido</strong><br><sub>El contexto que falta nunca se convierte silenciosamente en Permitido o Denegado.</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/es/access-light.png" alt="LAB\Alex recibe Modificar mediante LAB\Finance. Capturas nativas de un escenario sintético evaluado por Authz; sin datos empresariales reales ni resultados retocados." width="94%"></p>

## Empiece en dos minutos

1. Descargue el instalador completo o ZIP portátil desde Releases. Elija x64 para Intel/AMD o ARM64 para un PC ARM.
2. Instale para su cuenta o extraiga todo el ZIP y abra PermissionScope.exe.
3. Explore la demostración con LAB\Alex. Para sus archivos, elija Analizar e indique una carpeta.
4. Deje la identidad vacía para usar su token actual de Windows. Abra Access Path para revisar las pruebas.

## Del resultado a la evidencia

Windows Authz calcula la máscara. Access Path muestra las entradas contribuyentes y las relaciones registradas. La marca de herencia no demuestra el antecesor de origen. El control total en la ACL no garantiza abrir un archivo.

```text
Identidad de Windows
      ↓
SID + contexto de pertenencia registrado
      ↓
ACL / entradas de permisos discrecionales
      ↓
Evaluación con Windows Authz
      ↓
Permitido · Parcial · Denegado · Desconocido
      ↓
Access Path → evidencia contribuyente
```

## Interprete el resultado

Permitido autoriza la acción según las reglas discrecionales evaluadas. Parcial permite algunas acciones. Denegado no concede acceso. Desconocido indica que falta contexto para confirmar el resultado; nunca equivale a permitido.

## Compruebe la explicación

Windows Authz calcula la máscara. Access Path muestra las entradas contribuyentes y las relaciones registradas. La marca de herencia no demuestra el antecesor de origen. El control total en la ACL no garantiza abrir un archivo.

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/es/access-path-light.png" alt="Access Path" width="94%"></p>

## Superficie del producto

<table><tr><td><strong>Analizar</strong><br><sub>Inspeccione el acceso efectivo para una carpeta e identidad.</sub></td><td><strong>Explicar</strong><br><sub>Siga Access Path y la evidencia que contribuye.</sub></td><td><strong>Instantánea</strong><br><sub>Guarde observaciones locales para revisarlas después.</sub></td></tr><tr><td><strong>Comparar</strong><br><sub>Compare observaciones sin asumir que los recursos ausentes fueron eliminados.</sub></td><td><strong>Simular</strong><br><sub>Modele la eliminación de una regla en memoria antes de considerar un cambio real.</sub></td><td><strong>Exportar</strong><br><sub>Salidas HTML, CSV, JSON, XLSX y PDF.</sub></td></tr></table>

## Conserve las pruebas

Guarde instantáneas locales, compare observaciones, simule quitar una regla en memoria y exporte HTML, CSV, JSON, XLSX o PDF. Un recurso ausente en la observación posterior no se considera eliminado.

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## Alcance y privacidad

Los contextos remotos, S4U, reglas condicionales y destinos de enlaces no verificados siguen siendo Desconocidos. Integridad, cifrado, bloqueos y privilegios quedan fuera. Sin telemetría, cuentas ni nube. Las exportaciones reales pueden contener rutas y nombres sensibles.

> El análisis y la simulación son de solo lectura. Aplicar un cambio es un proceso separado, confirmado explícitamente, para archivos locales normales, con instantánea, registro, verificación y reversión. Se excluyen carpetas, enlaces y archivos con varios enlaces físicos.

## Descargue y verifique

Windows 10 1809 o posterior. Los paquetes completos incluyen los runtimes. Los instaladores aún no están firmados; compruebe SHA-256. ARM64 se compila en CI, sin certificación en hardware ARM. Consulte el estado para Store y WinGet.

[Descargas de la versión](https://github.com/MukaSanches/PermissionScope/releases/latest) · [Checksums SHA-256](https://github.com/MukaSanches/PermissionScope/releases/latest/download/SHA256SUMS.txt) · [Estado de la versión](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## Diseñado para verificarse

| Diseñado para verificarse | |
|---|---|
| Código fuente | Repositorio público e historial de commits |
| Releases | Artefactos versionados y checksums SHA-256 |
| Cadena de suministro | SBOM CycloneDX y automatización fijada cuando se documenta |
| Seguridad | Modelo de seguridad, divulgación responsable y CodeQL |
| Plataforma | Rutas de compilación x64 y ARM64 |
| Documentación | Ocho manuales localizados, guías visuales y cursos de Academy |
| Privacidad | Diseño local-first y guía explícita sobre sensibilidad de las exportaciones |

[Abrir Trust Center](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## Aprenda el modelo, no solo los botones

- [Fundamentos de permisos](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [Leer Access Path](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [Diagnóstico seguro](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [Manuales PDF · 8 idiomas](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## Elija el siguiente paso

- [Guía visual](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/es.md)
- [Modelo técnico](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [Preguntas y problemas](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [Glosario sencillo](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [Privacidad](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [Estado de la versión](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [Gobernanza](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [Soporte](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [Citación](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## Límites de ingeniería

PermissionScope no afirma que una ACL discrecional explique todos los resultados posibles al abrir archivos. Integridad, cifrado, bloqueos, algunos contextos remotos/S4U, reglas condicionales, privilegios administrativos y destinos reparse no verificados pueden quedar fuera del contexto disponible. Esos límites se documentan en lugar de ocultarse.

## Compilar y probar

Utilice Windows y .NET 10 SDK. Las pruebas son un ejecutable de integración, no dotnet test. Consulte la guía de desarrollo para empaquetar y verificar la documentación.

[Desarrollo](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [Rendimiento](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [Hoja de ruta](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## Ayuda y colaboración

Incluya versión, Windows, operación y código de error. La pestaña Técnica copia un diagnóstico sin rutas, cuentas ni SID. Las traducciones son preliminares; las pruebas técnicas pueden seguir en inglés. No se afirma revisión nativa ni certificación de tecnologías de asistencia.

---

<p align="center"><sub>Creado por Samuel Sanches · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
