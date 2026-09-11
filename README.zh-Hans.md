<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>让 Windows 权限真正可检查。</strong><br>了解谁能访问文件夹，并查看支持结论的权限规则。</p>
<table><tr><td align="center"><a href="https://mukasanches.github.io/PermissionScope/index.zh-Hans.html"><strong>网站</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/releases/latest"><strong>版本下载</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md"><strong>信任中心</strong></a></td></tr><tr><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">学院</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">路线图</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">安全</a></td></tr></table>

<p align="center"><sub>语言</sub></p>
<table><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.md"><strong>English</strong></a><br><sub>en-US</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md"><strong>Português</strong></a><br><sub>pt-BR</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md"><strong>Español</strong></a><br><sub>es</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md"><strong>Français</strong></a><br><sub>fr</sub></td></tr><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md"><strong>Deutsch</strong></a><br><sub>de</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md"><strong>العربية</strong></a><br><sub>ar</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md"><strong>日本語</strong></a><br><sub>ja</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md"><strong>简体中文</strong></a><br><sub>zh-Hans</sub></td></tr></table>

---

<table><tr><td width="33%"><strong>本地优先</strong><br><sub>无需 PermissionScope 账户、应用遥测或强制云服务。</sub></td><td width="33%"><strong>Windows 原生判定</strong><br><sub>有效访问由 Windows Authz 评估，而不是手写近似算法。</sub></td><td width="33%"><strong>未知保持未知</strong><br><sub>缺少上下文时绝不会静默变成允许或拒绝。</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/zh-Hans/access-light.png" alt="LAB\Alex 通过 LAB\Finance 获得修改权限。这些是由 Authz 评估合成场景的原生应用截图，不包含真实企业数据或经修改的结果。" width="94%"></p>

## 两分钟上手

1. 从 Releases 下载完整安装程序或便携 ZIP。Intel/AMD 选择 x64，ARM 电脑选择 ARM64。
2. 为当前帐户安装，或解压整个 ZIP 后打开 PermissionScope.exe。
3. 使用 LAB\Alex 探索演示。分析自己的文件时，选择分析并输入文件夹。
4. 将身份留空以使用当前 Windows 令牌。打开 Access Path 查看依据。

## 从结果到证据

Windows Authz 计算权限掩码。Access Path 显示有贡献的条目和已记录的成员关系。继承标志不能证明来源祖先。自主 ACL 中的完全控制也不保证能打开文件。

```text
Windows 身份
      ↓
SID + 已记录的成员关系上下文
      ↓
ACL / 自主权限条目
      ↓
Windows Authz 评估
      ↓
允许 · 部分允许 · 拒绝 · 未知
      ↓
Access Path → 贡献证据
```

## 理解结果

允许表示评估的自主访问控制规则允许指定操作。部分表示允许某些操作。拒绝表示这些规则不授予访问权限。未知表示上下文不足以确认结果，绝不当作允许。

## 核实解释

Windows Authz 计算权限掩码。Access Path 显示有贡献的条目和已记录的成员关系。继承标志不能证明来源祖先。自主 ACL 中的完全控制也不保证能打开文件。

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/zh-Hans/access-path-light.png" alt="Access Path" width="94%"></p>

## 产品能力

<table><tr><td><strong>分析</strong><br><sub>检查文件夹与身份的有效访问。</sub></td><td><strong>解释</strong><br><sub>沿 Access Path 查看贡献证据。</sub></td><td><strong>快照</strong><br><sub>保存本地观察，供之后复核。</sub></td></tr><tr><td><strong>比较</strong><br><sub>比较观察结果，不把缺失资源自动视为已删除。</sub></td><td><strong>模拟</strong><br><sub>真正修改前，先在内存中模拟移除规则。</sub></td><td><strong>导出</strong><br><sub>HTML、CSV、JSON、XLSX 和 PDF 输出。</sub></td></tr></table>

## 保留依据

保存本地快照、比较观测、在内存中模拟移除规则，并导出 HTML、CSV、JSON、XLSX 或 PDF。后续观测中未出现的资源不会被自动判定为已删除。

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## 范围与隐私

远程登录、S4U 上下文、条件规则和未经验证的链接目标仍为未知。完整性策略、加密、锁和特权不在本次判定范围内。无遥测、帐户或云服务。真实导出可能包含敏感路径和帐户名称。

> 分析和模拟为只读操作。应用更改是单独且需明确确认的流程，仅适用于普通本地文件，并包含快照、持久日志、验证和回滚。文件夹、链接及具有多个硬链接的文件除外。

## 下载并验证

Windows 10 1809 或更高版本。完整包包含运行时。安装程序目前未签名，请核对 SHA-256。ARM64 在 CI 中交叉构建，尚无 ARM 实机认证。Store 和 WinGet 可用性请查看版本状态。

[版本下载](https://github.com/MukaSanches/PermissionScope/releases/latest) · [SHA-256 校验和](https://github.com/MukaSanches/PermissionScope/releases/latest/download/SHA256SUMS.txt) · [版本状态](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## 为可验证性而构建

| 为可验证性而构建 | |
|---|---|
| 源代码 | 公开仓库与提交历史 |
| 发行版 | 版本化产物与 SHA-256 校验和 |
| 供应链 | CycloneDX SBOM 与文档中固定版本的自动化 |
| 安全 | 安全模型、披露指南与 CodeQL |
| 平台 | x64 与 ARM64 构建路径 |
| 文档 | 八种语言的手册、视觉指南与 Academy 课程 |
| 隐私 | 本地优先设计，并明确说明导出内容可能包含敏感信息 |

[打开 Trust Center](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## 学习模型，而不只是按钮

- [权限基础](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [阅读 Access Path](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [安全诊断](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [PDF 手册 · 8 种语言](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## 选择下一步

- [图文指南](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/zh-Hans.md)
- [技术模型](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [常见问题与故障排查](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [通俗术语表](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [隐私](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [版本状态](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [治理](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [支持](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [引用](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## 工程边界

PermissionScope 不声称随意 ACL 评估可以解释所有文件打开结果。完整性策略、加密、锁定、部分 Remote/S4U 上下文、条件规则、管理员权限影响以及未经验证的 reparse 目标可能超出可用判定上下文。这些边界会被明确记录，而不是隐藏。

## 构建与测试

使用 Windows 和 .NET 10 SDK。测试为可执行的集成测试程序，不是 dotnet test。打包和文档检查请参阅开发指南。

[开发](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [基准测试](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [路线图](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## 帮助与贡献

请提供版本、Windows 版本、操作和错误码。技术选项卡可复制不含路径、帐户或 SID 的诊断信息。翻译为预览版，技术依据可能仍为英文。不声称已完成母语审核或辅助技术认证。

---

<p align="center"><sub>由 Samuel Sanches 创建 · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
