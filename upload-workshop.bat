@echo off
chcp 65001 >nul
setlocal

rem ============================================================
rem  YunoMod 工坊上传脚本（依据 Tutorials\Basics\11 - 上传工坊）
rem
rem  本脚本只做两件事：把已部署的 mod 文件复制进工坊工作区，然后调用官方上传器上传。
rem  构建与 PCK 导出请先自行执行：
rem      dotnet build YunoMod.csproj
rem      dotnet msbuild YunoMod.csproj -t:ExportPck
rem
rem  首次运行会自动创建工作区（yuno-mod\workshop），之后直接把更新后的文件覆盖进去再上传即可。
rem ============================================================

rem ---- 可配置项 ----
set "STS2_DIR=D:\steam\steamapps\common\Slay the Spire 2"
set "SRC_DIR=%STS2_DIR%\mods\YunoMod"
set "UPLOADER=%~dp0ModUploader-win-x64\ModUploader.exe"
set "WORKSPACE=%~dp0workshop"

echo [信息] 工作区：%WORKSPACE%

rem ---- 0. 环境检查 ----
if not exist "%UPLOADER%" (
    echo [错误] 找不到上传器：%UPLOADER%
    echo        请从 https://github.com/megacrit/sts2-mod-uploader 下载后解压到 ModUploader-win-x64\
    pause
    exit /b 1
)

if not exist "%SRC_DIR%\YunoMod.json" (
    echo [错误] 找不到已部署的 mod 文件：%SRC_DIR%
    echo        请先构建并导出 PCK：
    echo          dotnet build YunoMod.csproj
    echo          dotnet msbuild YunoMod.csproj -t:ExportPck
    pause
    exit /b 1
)

for %%F in (YunoMod.dll YunoMod.json YunoMod.pck) do (
    if not exist "%SRC_DIR%\%%F" echo [警告] 缺少 %%F，请确认已构建并导出 PCK
)

rem ---- 1. 首次运行：创建工作区（模板含 content\、workshop.json、image.png）----
if not exist "%WORKSPACE%\workshop.json" (
    echo [信息] 未找到工作区，正在创建...
    "%UPLOADER%" new -w "%WORKSPACE%"
    if errorlevel 1 (
        echo [错误] 工作区创建失败
        pause
        exit /b 1
    )
)

rem ---- 2. 同步 mod 文件到 content\ ----
if not exist "%WORKSPACE%\content" mkdir "%WORKSPACE%\content"
del /q "%WORKSPACE%\content\*" >nul 2>nul
copy /y "%SRC_DIR%\YunoMod.dll"  "%WORKSPACE%\content\" >nul
copy /y "%SRC_DIR%\YunoMod.json" "%WORKSPACE%\content\" >nul
copy /y "%SRC_DIR%\YunoMod.pck"  "%WORKSPACE%\content\" >nul
echo [信息] 已复制到 content\：YunoMod.dll / YunoMod.json / YunoMod.pck

rem ---- 3. 首次上传前人工确认（tags 之后无法在工坊修改）----
if not exist "%WORKSPACE%\mod_id.txt" (
    echo.
    echo [提醒] 这是首次上传（工作区里还没有 mod_id.txt）。请先确认下面的配置：
    echo.
    type "%WORKSPACE%\workshop.json"
    echo.
    echo        建议：title / description / visibility / changeNote 留空或删除，上传后在工坊页面改；
    echo              只保留 tags（tags 无法在工坊修改），常用值如 "Characters","Cards","Relics","schinese"。
    echo       预览图：%WORKSPACE%\image.png（必须是 png 且小于 1MB，现在的还是模板占位图）
    echo.
    echo        确认无误后按任意键继续上传，或按 Ctrl+C 取消后先修改上面两个文件。
    pause >nul
)

rem ---- 4. 上传 ----
echo [信息] 开始上传...
"%UPLOADER%" upload -w "%WORKSPACE%"
set "RC=%ERRORLEVEL%"

echo.
if "%RC%"=="0" (
    echo [完成] 上传成功。
    echo        首次上传后工作区会生成 mod_id.txt，之后本脚本会自动更新同一个工坊项目。
) else (
    echo [错误] 上传失败，退出码 %RC%（常见原因：Steam 未启动/未登录、image.png 超过 1MB）
)

pause
endlocal