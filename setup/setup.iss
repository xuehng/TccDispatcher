;#define use_iis
;#define use_kb835732

;#define use_msi20
#define use_msi31
;#define use_msi45

;#define use_ie6

;#define use_dotnetfx11
;#define use_dotnetfx11lp

;#define use_dotnetfx20
;#define use_dotnetfx20lp

;#define use_dotnetfx35
;#define use_dotnetfx35lp

#define use_dotnetfx40
#define use_wic

;#define use_vc2010

;#define use_mdac28
;#define use_jet4sp8

;#define use_sqlcompact35sp2

;#define use_sql2005express
;#define use_sql2008express

#define src_dir '..\bin\Release'

#define MyAppSetupName 'Superdisp'
#define MyAppExeName "SupernovaDispatcher.exe"
#define MyAppVersion '2.0.0.0'
#define MyAppVendorName 'trinnolink'

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright=Copyright Renstech 2007-2012
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppVendorName}
AppPublisher={#MyAppVendorName}
;AppPublisherURL=http://...
;AppSupportURL=http://...
;AppUpdatesURL=http://...
OutputBaseFilename={#MyAppSetupName}-{#MyAppVersion}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={pf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
;SetupIconFile=MyProgramIcon
SolidCompression=yes

;MinVersion default value: "0,5.0 (Windows 2000+) if Unicode Inno Setup, else 4.0,4.0 (Windows 95+)"
;MinVersion=0,5.0
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
;Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#src_dir}\Windows7X64\*"; DestDir: "{app}\Windows7X64"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#src_dir}\Windows7X86\*"; DestDir: "{app}\Windows7X86"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#src_dir}\WindowsXP\*"; DestDir: "{app}\WindowsXP"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#src_dir}\clrzmq.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Fintek.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\CookComputing.XmlRpcServerV2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\ProgressControls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\CookComputing.XmlRpcV2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\CoreAudioApi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\libzmq.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\LumiSoft.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Microsoft.Expression.Drawing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\sip_net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\sipdll.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\LitePhone.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\InterProcPipe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\NIDSLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\ftplib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\SupernovaDispatcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Vlc.DotNet.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Vlc.DotNet.Core.Interops.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Vlc.DotNet.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\WPFSoundVisualizationLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\WPFToolkit.Extended.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Xceed.Wpf.Themes.Office2007.v2.0.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Xceed.Wpf.Themes.Media.v2.0.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Xceed.Wpf.Themes.Glass.v2.0.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Xceed.Wpf.Themes.LiveExplorer.v2.0.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Xceed.Wpf.Themes.Windows7.v2.0.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Xceed.Wpf.Themes.v2.0.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\DirectPort.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\GPIO6854.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\GPIO.NET.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Novell.Directory.Ldap.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Mono.Security.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Google.ProtocolBuffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\Google.ProtocolBuffers.Serialization.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\log.cfg"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#src_dir}\rings\*"; DestDir: "{app}\rings\"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppSetupName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon


#include "scripts\products.iss"

#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"

#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif

#ifdef use_msi20
#include "scripts\products\msi20.iss"
#endif
#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif
#ifdef use_msi45
#include "scripts\products\msi45.iss"
#endif

#ifdef use_ie6
#include "scripts\products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "scripts\products\dotnetfx11.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#ifdef use_dotnetfx11lp
#include "scripts\products\dotnetfx11lp.iss"
#endif
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#include "scripts\products\dotnetfx20sp1lp.iss"
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
;#include "scripts\products\dotnetfx35.iss"
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
;#include "scripts\products\dotnetfx35lp.iss"
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif

#ifdef use_wic
#include "scripts\products\wic.iss"
#endif

#ifdef use_vc2010
#include "scripts\products\vcredist2010.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif

#ifdef use_sqlcompact35sp2
#include "scripts\products\sqlcompact35sp2.iss"
#endif

#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2


[Code]
function InitializeSetup(): boolean;
begin
	//init windows version
	initwinversion();

#ifdef use_iis
	if (not iis()) then exit;
#endif

#ifdef use_msi20
	msi20('2.0');
#endif
#ifdef use_msi31
	msi31('3.1');
#endif
#ifdef use_msi45
	msi45('4.5');
#endif
#ifdef use_ie6
	ie6('5.0.2919');
#endif

#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif

	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	//check if .netfx 2.0 can be installed on this OS
	if not minwinspversion(5, 0, 3) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['2000', '3'])]), mberror, mb_ok);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['XP', '2'])]), mberror, mb_ok);
		exit;
	end;

	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif

#ifdef use_dotnetfx35
	//dotnetfx35();
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	//dotnetfx35lp();
	dotnetfx35sp1lp();
#endif
#endif

	// if no .netfx 4.0 is found, install the client (smallest)
#ifdef use_dotnetfx40
	if (not netfxinstalled(NetFx40Client, '') and not netfxinstalled(NetFx40Full, '')) then
		dotnetfx40client();
#endif

#ifdef use_wic
	wic();
#endif

#ifdef use_vc2010
	vcredist2010();
#endif

#ifdef use_mdac28
	mdac28('2.7');
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015');
#endif

#ifdef use_sqlcompact35sp2
	sqlcompact35sp2();
#endif

#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif

	Result := true;
end;
