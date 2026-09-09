# ============================================================
#  RBS.Api 镜像（多阶段构建）
#
#  ⚠️ 构建上下文必须是【仓库根目录】：
#      docker build -f src/RBS.Api/Dockerfile -t rbs-api:latest .
#
#  原因：RBS.Infrastructure.csproj 用 HintPath 直接引用仓库根的
#        lib/itextsharp.dll、lib/QRCoder.dll（路径 ..\..\lib\），
#        必须先 COPY lib/ 到对应位置，否则 restore 报 NU1101。
#
#  端口：容器内监听 8080（由 Deployment 的 ASPNETCORE_URLS 环境变量控制）
#
#  ⚠️ PDF 生成已跨平台化（NotePdf.cs 不再使用 System.Drawing，
#     二维码走 PngByteQRCode、条形码走 iTextSharp PlaceBarcode），
#     aspnet:10.0（Debian）镜像无需额外安装 libgdiplus，也不带 curl。
# ============================================================

# ---- 阶段 1：SDK 构建 ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) 只拷各项目 csproj + lib/，先 restore（利用层缓存：改代码不重下 NuGet 包）
COPY src/RBS.Api/RBS.Api.csproj src/RBS.Api/
COPY src/RBS.Application/RBS.Application.csproj src/RBS.Application/
COPY src/RBS.Core/RBS.Core.csproj src/RBS.Core/
COPY src/RBS.Infrastructure/RBS.Infrastructure.csproj src/RBS.Infrastructure/
COPY lib/ lib/

RUN dotnet restore src/RBS.Api/RBS.Api.csproj

# 2) 拷全部源码并发布。
#    SqlMaps.xml / audit-field-config.json / Content\Fonts\SimHei.ttf
#    均由各自 csproj 的 CopyToOutputDirectory 自动带入发布产物
COPY src/ src/
RUN dotnet publish src/RBS.Api/RBS.Api.csproj -c Release -o /app/publish

# ---- 阶段 2：ASP.NET Core 运行时 ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RBS.Api.dll"]
