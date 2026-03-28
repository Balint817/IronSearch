@echo off

if "%~1"=="" (
    echo Usage: optimize_gif.bat input.gif
    exit /b
)
set INPUT=%~1
set OUTPUT=%~n1_optimized.gif

ffmpeg -y -i "%INPUT%" -vf "fps=12,scale=480:-1:flags=lanczos,palettegen" palette.png

ffmpeg -i "%INPUT%" -i palette.png -filter_complex "fps=12,scale=480:-1:flags=lanczos[x];[x][1:v]paletteuse" -loop 0 "%OUTPUT%"

if exist "palette.png" del /q "palette.png"