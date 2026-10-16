@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo === create orphan branch ===
git checkout --orphan slim-main
if errorlevel 1 exit /b 1

echo === restage all files with new gitignore ===
git reset
git add -A
if errorlevel 1 exit /b 1

echo === staged size preview ===
git diff --cached --shortstat

echo === commit ===
git commit -m "chore: slim repo for GitHub (exclude 100MB+ assets)"
if errorlevel 1 exit /b 1

echo === replace main ===
git branch -M main
if errorlevel 1 exit /b 1

echo === done ===
git log -1 --oneline
git count-objects -vH
git status -sb
