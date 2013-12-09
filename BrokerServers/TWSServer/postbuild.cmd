@echo off
echo using target dir: %1
if exist TwsServer.Config.txt (
copy TwsServer.Config.txt %1
) else (
echo TWSServer.Config.txt not found.
)
if exist SymbolOverrides.csv (
copy SymbolOverrides.csv %1
) else (
echo SymbolOverrides.txt not found.
)