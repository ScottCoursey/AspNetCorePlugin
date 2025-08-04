SET SolutionDir=%1%

SET DebugOutputDir=%SolutionDir%\AspNetCorePlugin\bin\Debug\net8.0
SET DebugPluginOutputDir=%DebugOutputDir%\Plugins

SET ReleaseOutputDir=%SolutionDir%\AspNetCorePlugin\bin\Release\net8.0
SET ReleasePluginOutputDir=%ReleaseOutputDir%\Plugins

IF EXIST %DebugOutputDir% IF NOT EXIST %DebugPluginOutputDir% MKDIR %DebugPluginOutputDir%
IF EXIST %DebugPluginOutputDir% COPY %SolutionDir%\AspNetCorePlugin\ClientPlugin.dll %DebugPluginOutputDir%

IF EXIST %ReleaseOutputDir% IF NOT EXIST %ReleasePluginOutputDir% MKDIR %ReleasePluginOutputDir%
IF EXIST %ReleasePluginOutputDir% COPY %SolutionDir%\AspNetCorePlugin\ClientPlugin.dll %ReleasePluginOutputDir%

