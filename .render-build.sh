#!/usr/bin/env bash

# Vue 前端 build
cd ClientApp
npm install
npm run build

# 回到根目錄，build .NET
cd ..
dotnet publish -c Release -o ./publish