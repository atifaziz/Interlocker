@echo off
pushd "%~dp0"
call build ^
    && NuGet pack Interlocker.nuspec -Symbol ^
    && NuGet pack Interlocker.Source.nuspec
popd
goto :EOF
