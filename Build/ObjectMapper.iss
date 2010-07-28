[Setup]
AppName=ObjectMapper .NET
AppVerName=ObjectMapper .NET @build.version@
AppPublisher=Gerhard Stephan
AppPublisherURL=http://www.objectmapper.net
AppSupportURL=http://www.objectmapper.net
AppUpdatesURL=http://www.objectmapper.net
DefaultDirName={pf}\ObjectMapper
DefaultGroupName=ObjectMapper .NET
AllowNoIcons=yes
OutputDir=@output.dir@
OutputBaseFilename=@output.file@
Compression=lzma
SolidCompression=yes

[Files]
Source: @release.dir@\*.*; DestDir: {app}; Flags: ignoreversion recursesubdirs

[INI]
Filename: {app}\ObjectMapper.url; Section: InternetShortcut; Key: URL; String: http://www.objectmapper.net
Filename: {app}\ObjectMapperBlog.url; Section: InternetShortcut; Key: URL; String: http://blog.objectmapper.net
Filename: {app}\Tutorials.url; Section: InternetShortcut; Key: URL; String: http://blog.objectmapper.net/tutorials/

[Icons]
Name: {group}\ObjectMapper .NET 2005; Filename: {app}\ObjectMapper .NET 2005.sln; WorkingDir: {app}
Name: {group}\ObjectMapper .NET 2008; Filename: {app}\ObjectMapper .NET 2008.sln; WorkingDir: {app}
Name: {group}\ObjectMapper .NET CE; Filename: {app}\ObjectMapper .NET CE.sln; WorkingDir: {app}
Name: {group}\Change Log; FileName: {app}\changelog.txt; WorkingDir: {app}
Name: {group}\{cm:ProgramOnTheWeb,ObjectMapper .NET}; Filename: {app}\ObjectMapper.url
Name: {group}\{cm:ProgramOnTheWeb,ObjectMapper .NET Blog}; Filename: {app}\ObjectMapperBlog.url
Name: {group}\SDK ObjectMapper .NET; Filename: {app}\Documentation\ObjectMapper.chm; WorkingDir: {app}\Documentation\html\
Name: {group}\Tutorials\{cm:ProgramOnTheWeb,ObjectMapper .NET Tutorials}; Filename: {app}\Tutorials.url
Name: {group}\Tutorials\Tutorial 01; Filename: {app}\Tutorial01\Tutorial01\Tutorial01.sln; WorkingDir: {app}
Name: {group}\Tutorials\Tutorial 02; Filename: {app}\Tutorial02\Tutorial02\Tutorial02.sln; WorkingDir: {app}
Name: {group}\Tutorials\Tutorial 03; Filename: {app}\Tutorial03\Tutorial03\Tutorial03.sln; WorkingDir: {app}
Name: {group}\Tutorials\Tutorial 04; Filename: {app}\Tutorial04\Tutorial04\Tutorial04.sln; WorkingDir: {app}

[UninstallDelete]
Type: filesandordirs; Name: {app}\*.*

