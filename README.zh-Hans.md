<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. -->
# PermissionScope

[English](README.md) · [Português](README.pt-BR.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [العربية](README.ar.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md)

了解谁能访问文件夹，并查看支持结论的权限规则。

[版本下载](https://github.com/MukaSanches/PermissionScope/releases) · [图文指南](docs/guides/zh-Hans.md) · [试用合成 HTML 报告](https://mukasanches.github.io/PermissionScope/reports/permissionscope-demo.html)

![LAB\Alex 通过 LAB\Finance 获得修改权限。这些是由 Authz 评估合成场景的原生应用截图，不包含真实企业数据或经修改的结果。](docs/screenshots/zh-Hans/access-light.png)

## 两分钟上手

1. 从 Releases 下载完整安装程序或便携 ZIP。Intel/AMD 选择 x64，ARM 电脑选择 ARM64。
2. 为当前帐户安装，或解压整个 ZIP 后打开 PermissionScope.exe。
3. 使用 LAB\Alex 探索演示。分析自己的文件时，选择分析并输入文件夹。
4. 将身份留空以使用当前 Windows 令牌。打开 Access Path 查看依据。

## 理解结果

允许表示评估的自主访问控制规则允许指定操作。部分表示允许某些操作。拒绝表示这些规则不授予访问权限。未知表示上下文不足以确认结果，绝不当作允许。

## 核实解释

Windows Authz 计算权限掩码。Access Path 显示有贡献的条目和已记录的成员关系。继承标志不能证明来源祖先。自主 ACL 中的完全控制也不保证能打开文件。

![LAB\Alex 通过 LAB\Finance 获得修改权限。这些是由 Authz 评估合成场景的原生应用截图，不包含真实企业数据或经修改的结果。](docs/screenshots/zh-Hans/access-path-light.png)

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

分析和模拟为只读操作。应用更改是单独且需明确确认的流程，仅适用于普通本地文件，并包含快照、持久日志、验证和回滚。文件夹、链接及具有多个硬链接的文件除外。

## 下载并验证

Windows 10 1809 或更高版本。完整包包含运行时。安装程序目前未签名，请核对 SHA-256。ARM64 在 CI 中交叉构建，尚无 ARM 实机认证。Store 和 WinGet 可用性请查看版本状态。

## 选择下一步

- [图文指南](docs/guides/zh-Hans.md)
- [技术模型](docs/access-model.md)
- [常见问题与故障排查](docs/faq.md)
- [通俗术语表](docs/glossary.md)
- [隐私](docs/privacy.md)
- [版本状态](docs/release-status.md)

## 构建与测试

使用 Windows 和 .NET 10 SDK。测试为可执行的集成测试程序，不是 dotnet test。打包和文档检查请参阅开发指南。

[Development](docs/development.md)

## 帮助与贡献

请提供版本、Windows 版本、操作和错误码。技术选项卡可复制不含路径、帐户或 SID 的诊断信息。翻译为预览版，技术依据可能仍为英文。不声称已完成母语审核或辅助技术认证。

由 Samuel Sanches 创建 · ssanches011@gmail.com · [Apache-2.0](LICENSE) · [Security](SECURITY.md)
