<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.svg" alt="PermissionScope" width="100%"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>أذونات Windows بشكل يمكن فحصه.</strong><br>افهم من يمكنه الوصول إلى مجلد، وافحص القواعد التي تفسر النتيجة.</p>
<p align="center"><a href="https://mukasanches.github.io/PermissionScope/index.ar.html">Website</a> · <a href="https://github.com/MukaSanches/PermissionScope/releases/latest">تنزيلات الإصدار</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md">Trust Center</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">Academy</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">Roadmap</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a></p>

<p align="center">[English](https://github.com/MukaSanches/PermissionScope/blob/main/README.md) · [Português](https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md) · [Español](https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md) · [Français](https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md) · [Deutsch](https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md) · [العربية](https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md) · [日本語](https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md) · [简体中文](https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md)</p>

---

<table><tr><td width="33%"><strong>محلي أولاً</strong><br><sub>لا حساب PermissionScope ولا قياس عن بُعد للتطبيق ولا خدمة سحابية إلزامية.</sub></td><td width="33%"><strong>قرار أصلي من Windows</strong><br><sub>يتم تقييم الوصول الفعلي بواسطة Windows Authz بدلاً من تقريب مكتوب يدوياً.</sub></td><td width="33%"><strong>غير معروف يبقى غير معروف</strong><br><sub>السياق المفقود لا يتحول بصمت إلى مسموح أو مرفوض.</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/ar/access-light.png" alt="يحصل LAB\Alex على التعديل عبر LAB\Finance. لقطات أصلية لسيناريو اصطناعي يقيّمه Authz، بلا بيانات شركة حقيقية أو نتائج معدّلة." width="94%"></p>

## ابدأ خلال دقيقتين

1. نزّل المثبّت الكامل أو ZIP المحمول من Releases. اختر x64 لأجهزة Intel/AMD أو ARM64 لأجهزة ARM.
2. ثبّت لحسابك أو استخرج ZIP كاملًا وافتح PermissionScope.exe.
3. استكشف العرض باستخدام LAB\Alex. لملفاتك، اختر تحليل وحدد مجلدًا.
4. اترك الهوية فارغة لاستخدام رمز Windows الحالي. افتح Access Path لفحص الأدلة.

## من النتيجة إلى الدليل

يحسب Windows Authz قناع الأذونات. يعرض Access Path القواعد المساهمة والعضويات المسجلة. علامة التوريث لا تثبت المجلد الأصلي. التحكم الكامل في ACL لا يضمن فتح الملف.

```text
Windows identity
      ↓
SID + recorded membership context
      ↓
ACL / discretionary permission entries
      ↓
Windows Authz evaluation
      ↓
Granted · Partial · Denied · Unknown
      ↓
Access Path → contributing evidence
```

## افهم النتيجة

مسموح يتيح الإجراء وفق قواعد الوصول التقديرية التي تم تقييمها. جزئي يتيح بعض الإجراءات. مرفوض لا يمنح وصولًا. غير معروف يعني أن السياق لا يكفي لتأكيد النتيجة؛ ولا يُعامل كمسموح مطلقًا.

## تحقق من التفسير

يحسب Windows Authz قناع الأذونات. يعرض Access Path القواعد المساهمة والعضويات المسجلة. علامة التوريث لا تثبت المجلد الأصلي. التحكم الكامل في ACL لا يضمن فتح الملف.

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/ar/access-path-light.png" alt="Access Path" width="94%"></p>

## سطح المنتج

<table><tr><td><strong>تحليل</strong><br><sub>افحص الوصول الفعلي لمجلد وهوية.</sub></td><td><strong>شرح</strong><br><sub>اتبع Access Path والأدلة المساهمة.</sub></td><td><strong>لقطة</strong><br><sub>احفظ الملاحظات المحلية للمراجعة لاحقاً.</sub></td></tr><tr><td><strong>مقارنة</strong><br><sub>قارن الملاحظات دون افتراض حذف الموارد المفقودة.</sub></td><td><strong>محاكاة</strong><br><sub>حاكي إزالة قاعدة في الذاكرة قبل التفكير في تغيير فعلي.</sub></td><td><strong>تصدير</strong><br><sub>مخرجات HTML وCSV وJSON وXLSX وPDF.</sub></td></tr></table>

## احتفظ بالأدلة

احفظ لقطات محلية، وقارن الملاحظات، وحاكِ إزالة قاعدة في الذاكرة، وصدّر HTML أو CSV أو JSON أو XLSX أو PDF. غياب مورد في ملاحظة لاحقة لا يثبت حذفه.

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## النطاق والخصوصية

تظل عمليات الدخول البعيدة وسياقات S4U والقواعد الشرطية وأهداف الروابط غير المتحقق منها غير معروفة. سياسات السلامة والتشفير والأقفال والامتيازات خارج القرار. بلا قياس استخدام أو حساب أو سحابة. قد تتضمن التقارير الحقيقية مسارات وأسماء حساسة.

> التحليل والمحاكاة للقراءة فقط. تطبيق التغيير مسار منفصل بتأكيد صريح، للملفات المحلية العادية، مع لقطة وسجل دائم وتحقق وتراجع. المجلدات والروابط والملفات ذات الروابط الصلبة المتعددة مستثناة.

## نزّل وتحقق

Windows 10 1809 أو أحدث. تتضمن الحزم الكاملة بيئات التشغيل. المثبّتات غير موقّعة حاليًا؛ تحقق من SHA-256. يُبنى ARM64 في CI دون اعتماد على جهاز ARM فعلي. راجع حالة الإصدار لتوفر Store وWinGet.

[تنزيلات الإصدار](https://github.com/MukaSanches/PermissionScope/releases/latest) · [SHA-256](https://github.com/MukaSanches/PermissionScope/releases/latest) · [حالة الإصدار](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## مصمم للتحقق

| مصمم للتحقق | |
|---|---|
| المصدر | مستودع عام وسجل الالتزامات |
| الإصدارات | ملفات إصدار ببصمات SHA-256 |
| سلسلة التوريد | SBOM بصيغة CycloneDX وأتمتة مثبتة حيث تم توثيقها |
| الأمان | نموذج أمان وإرشادات إفصاح وCodeQL |
| المنصة | مسارات بناء x64 وARM64 |
| الوثائق | ثمانية أدلة مترجمة وأدلة مرئية ودورات Academy |
| الخصوصية | تصميم محلي أولاً وإرشادات واضحة لحساسية بيانات التصدير |

[فتح Trust Center](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## تعلم النموذج وليس الأزرار فقط

- [أساسيات الأذونات](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [قراءة Access Path](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [تشخيص آمن](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [Handbooks PDF · 8 languages](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## اختر الخطوة التالية

- [دليل مرئي](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/ar.md)
- [النموذج التقني](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [الأسئلة واستكشاف الأخطاء](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [مسرد مبسّط](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [الخصوصية](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [حالة الإصدار](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [Governance](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [Support](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [Citation](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## حدود هندسية

لا يدّعي PermissionScope أن تقييم ACL التقديري يفسر كل نتيجة ممكنة لفتح الملفات. قد تبقى سياسة التكامل والتشفير والأقفال وبعض سياقات Remote/S4U والقواعد الشرطية وامتيازات الإدارة وأهداف reparse غير الموثقة خارج السياق المتاح. يتم توثيق هذه الحدود بدلاً من إخفائها.

## البناء والاختبار

استخدم Windows و.NET 10 SDK. الاختبارات برنامج تكامل قابل للتنفيذ وليست dotnet test. راجع دليل التطوير للتغليف والتحقق من الوثائق.

[Development](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [Benchmarks](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [Roadmap](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## المساعدة والمساهمة

أرفق الإصدار ونسخة Windows والعملية ورمز الخطأ. تنسخ علامة التبويب التقنية تشخيصًا بلا مسارات أو حسابات أو SID. الترجمات أولية؛ قد تبقى الأدلة التقنية بالإنجليزية. لا ندّعي مراجعة ناطق أصلي أو اعتماد تقنيات مساعدة.

---

<p align="center"><sub>أنشأه Samuel Sanches · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
