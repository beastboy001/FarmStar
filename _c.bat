@echo off
cd /d "%~dp0"
git add -A
git commit -m "chore: remove temp slim scripts"
git log -2 --oneline
git status -sb
del /f /q "%~f0"
