<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. -->
# PermissionScope

[English](README.md) · [Português](README.pt-BR.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [العربية](README.ar.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md)

افهم من يمكنه الوصول إلى مجلد، وافحص القواعد التي تفسر النتيجة.

[تنزيلات الإصدار](https://github.com/MukaSanches/PermissionScope/releases) · [دليل مرئي](docs/guides/ar.md) · [جرّب تقرير HTML الاصطناعي](https://mukasanches.github.io/PermissionScope/reports/permissionscope-demo.html)

![يحصل LAB\Alex على التعديل عبر LAB\Finance. لقطات أصلية لسيناريو اصطناعي يقيّمه Authz، بلا بيانات شركة حقيقية أو نتائج معدّلة.](docs/screenshots/ar/access-light.png)

## ابدأ خلال دقيقتين

1. نزّل المثبّت الكامل أو ZIP المحمول من Releases. اختر x64 لأجهزة Intel/AMD أو ARM64 لأجهزة ARM.
2. ثبّت لحسابك أو استخرج ZIP كاملًا وافتح PermissionScope.exe.
3. استكشف العرض باستخدام LAB\Alex. لملفاتك، اختر تحليل وحدد مجلدًا.
4. اترك الهوية فارغة لاستخدام رمز Windows الحالي. افتح Access Path لفحص الأدلة.

## افهم النتيجة

مسموح يتيح الإجراء وفق قواعد الوصول التقديرية التي تم تقييمها. جزئي يتيح بعض الإجراءات. مرفوض لا يمنح وصولًا. غير معروف يعني أن السياق لا يكفي لتأكيد النتيجة؛ ولا يُعامل كمسموح مطلقًا.

## تحقق من التفسير

يحسب Windows Authz قناع الأذونات. يعرض Access Path القواعد المساهمة والعضويات المسجلة. علامة التوريث لا تثبت المجلد الأصلي. التحكم الكامل في ACL لا يضمن فتح الملف.

![يحصل LAB\Alex على التعديل عبر LAB\Finance. لقطات أصلية لسيناريو اصطناعي يقيّمه Authz، بلا بيانات شركة حقيقية أو نتائج معدّلة.](docs/screenshots/ar/access-path-light.png)

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

التحليل والمحاكاة للقراءة فقط. تطبيق التغيير مسار منفصل بتأكيد صريح، للملفات المحلية العادية، مع لقطة وسجل دائم وتحقق وتراجع. المجلدات والروابط والملفات ذات الروابط الصلبة المتعددة مستثناة.

## نزّل وتحقق

Windows 10 1809 أو أحدث. تتضمن الحزم الكاملة بيئات التشغيل. المثبّتات غير موقّعة حاليًا؛ تحقق من SHA-256. يُبنى ARM64 في CI دون اعتماد على جهاز ARM فعلي. راجع حالة الإصدار لتوفر Store وWinGet.

## اختر الخطوة التالية

- [دليل مرئي](docs/guides/ar.md)
- [النموذج التقني](docs/access-model.md)
- [الأسئلة واستكشاف الأخطاء](docs/faq.md)
- [مسرد مبسّط](docs/glossary.md)
- [الخصوصية](docs/privacy.md)
- [حالة الإصدار](docs/release-status.md)

## البناء والاختبار

استخدم Windows و.NET 10 SDK. الاختبارات برنامج تكامل قابل للتنفيذ وليست dotnet test. راجع دليل التطوير للتغليف والتحقق من الوثائق.

[Development](docs/development.md)

## المساعدة والمساهمة

أرفق الإصدار ونسخة Windows والعملية ورمز الخطأ. تنسخ علامة التبويب التقنية تشخيصًا بلا مسارات أو حسابات أو SID. الترجمات أولية؛ قد تبقى الأدلة التقنية بالإنجليزية. لا ندّعي مراجعة ناطق أصلي أو اعتماد تقنيات مساعدة.

أنشأه Samuel Sanches · ssanches011@gmail.com · [Apache-2.0](LICENSE) · [Security](SECURITY.md)
