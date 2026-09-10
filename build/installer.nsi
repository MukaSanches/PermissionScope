Unicode True
!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "WinVer.nsh"
!include "x64.nsh"
!ifndef ARCH
!define ARCH "x64"
!endif
!define VERSION "1.0.0"
!define PRODUCT "PermissionScope"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\PermissionScope"
Name "${PRODUCT}"
OutFile "..\artifacts\PermissionScope-${VERSION}-${ARCH}-Setup.exe"
InstallDir "$LOCALAPPDATA\Programs\PermissionScope"
InstallDirRegKey HKCU "${UNINSTALL_KEY}" "InstallLocation"
RequestExecutionLevel user
SetCompressor /SOLID lzma
VIProductVersion "1.0.0.0"
VIAddVersionKey "ProductName" "PermissionScope"
VIAddVersionKey "FileDescription" "PermissionScope installer"
VIAddVersionKey "FileVersion" "1.0.0"
VIAddVersionKey "LegalCopyright" "Copyright 2026 Samuel Sanches"
!define MUI_ICON "..\assets\PermissionScope.ico"
!define MUI_UNICON "..\assets\PermissionScope.ico"
!define MUI_ABORTWARNING
!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\PermissionScope.exe"
!define MUI_FINISHPAGE_RUN_NOTCHECKED
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "PortugueseBR"
!insertmacro MUI_LANGUAGE "Spanish"
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "German"
Function .onInit
  ${IfNot} ${AtLeastWin10}
    MessageBox MB_ICONSTOP "PermissionScope requires Windows 10 or later."
    Abort
  ${EndIf}
  ${IfNot} ${AtLeastBuild} 17763
    MessageBox MB_ICONSTOP "PermissionScope requires Windows 10 version 1809 or later."
    Abort
  ${EndIf}
!if "${ARCH}" == "arm64"
  ${IfNot} ${IsNativeARM64}
    MessageBox MB_ICONSTOP "This installer requires Windows on ARM64. Use the x64 download for Intel and AMD computers."
    Abort
  ${EndIf}
!else
  ${IfNot} ${RunningX64}
    MessageBox MB_ICONSTOP "This installer requires 64-bit Windows."
    Abort
  ${EndIf}
!endif
  SetShellVarContext current
FunctionEnd
Section "PermissionScope"
  SetOutPath "$INSTDIR"
  File /r "..\artifacts\PermissionScope-1.0.0-win-${ARCH}\*.*"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  CreateDirectory "$SMPROGRAMS\PermissionScope"
  CreateShortcut "$SMPROGRAMS\PermissionScope\PermissionScope.lnk" "$INSTDIR\PermissionScope.exe"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayName" "PermissionScope"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayVersion" "1.0.0"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "Publisher" "Samuel Sanches"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayIcon" "$INSTDIR\PermissionScope.exe"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKCU "${UNINSTALL_KEY}" "QuietUninstallString" '"$INSTDIR\Uninstall.exe" /S'
  WriteRegDWORD HKCU "${UNINSTALL_KEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTALL_KEY}" "NoRepair" 1
SectionEnd
Section "Uninstall"
  SetShellVarContext current
  !include "..\artifacts\uninstall-${ARCH}.nsh"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  Delete "$SMPROGRAMS\PermissionScope\PermissionScope.lnk"
  RMDir "$SMPROGRAMS\PermissionScope"
  DeleteRegKey HKCU "${UNINSTALL_KEY}"
SectionEnd
