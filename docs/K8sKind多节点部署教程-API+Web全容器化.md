# Windows 11 + Docker Desktop + kind：双域名 7 节点全容器化部署手册

> 适用：RentalBillingSystem 本地全容器化部署。**后端 RBS.Api 与前端 web 都进 kind 7 节点集群（Pod 方式部署）**，数据库继续用本机 Windows 上的 SQL Server。
>
> 前置目标：
> - **web 站点**：浏览器访问 `http://www.rbsadmin.com` 打开系统并登录
> - **api 站点**：浏览器访问 `http://www.rbsapi.com` 直接打开 API（Swagger）
> - 登录后能正常**预览 / 下载 PDF**（缴款通知书、催缴通知书）

---

## 0. 架构总览

**集群规模：7 个节点**（1 控制平面 + 6 worker），两个站点都通过 **NGINX Ingress 按域名路由**，API 与 Web 都以 **Pod** 方式调度到 worker 节点上。**6 个业务 Pod（API×3 + Web×3）恰好每 worker 节点 1 个**。

```
浏览器
   │
   ├── http://www.rbsapi.com   ──► 127.0.0.1:80 ──► kind 控制面节点（ingress-nginx）
   │                                                                    │ host 匹配
   │                                                                    ▼
   │                                                          rbs-api-svc（ClusterIP :8080）
   │                                                                    ▼
   │                                                          RBS.Api Pod × 3（replicas=3）
   │
   └── http://www.rbsadmin.com ──► 127.0.0.1:80 ──► kind 控制面节点（ingress-nginx）
                                                                        │ host 匹配
                                                                        ▼
                                                              rbs-web-svc（NodePort 80 / 兜底 30883）
                                                                        │
                                                                        ▼
                                                              web Pod × 3（nginx: web-web:latest）
                                                                        │  /api/* 反代（集群内 DNS）
                                                                        ▼
                                                              rbs-api-svc（ClusterIP :8080）
                                                                        ▼
                                                              RBS.Api Pod → hostAliases → 本机 SQL(1433)
```

> 路径说明：
> - **web → API 走集群内 Service**（`web/nginx.conf` 的 `proxy_pass http://rbs-api-svc:8080`），浏览器访问 `www.rbsadmin.com` 时 `/api/*` 由 web Pod 的 nginx 直接转发，不必绕回 ingress。
> - **API 站点独立可达**：`www.rbsapi.com` 由 ingress 直接路由到 `rbs-api-svc`，可直接开 Swagger、直接调接口（curl 验证很方便）。
> - **域名靠 hosts 文件解析**：`www.rbsapi.com` / `www.rbsadmin.com` 都指向 `127.0.0.1`（阶段 0 配置）；ingress 监听宿主机 80/443，所以访问的是干净的无端口 URL。
> - **兜底入口**：`http://localhost:30883`（NodePort）仍可直达 web，ingress 出问题时用。

**节点分配（7 节点，6 个 worker 全部可用）：**

| 节点 | 角色 | 承载 |
|---|---|---|
| rbs-control-plane | 控制平面（有 NoSchedule 污点） | 集群控制面 + **ingress-nginx 控制器**（hostPort 80/443） |
| rbs-worker ~ rbs-worker6 | worker × 6 | **API Pod × 3 + web Pod × 3，每 worker 恰好 1 个**（另：ingress 的 admission Job 临时落点由调度器决定） |

> `api` 3 副本 + `web` 3 副本 = 6 个业务 Pod，恰好铺满 6 个 worker，**每个节点最多 1 个业务 Pod**（`kubectl get pods -o wide` 的 NODE 列可看到一一对应）。

**为什么这样设计（重要约束）：**

| 约束 | 原因 |
|---|---|
| 7 节点 = 1 控制面 + 6 worker | kind 的控制面带 NoSchedule 污点，业务 Pod 只会调度到 worker；6 个业务 Pod 恰好每 worker 1 个 |
| API `replicas: 3`（2026-09-01 起） | 曾因调度生成"先查后插"限制为 1；现已把 `SqlMaps.xml` 的 `Scheduling.Insert.Execution.Default` 改为**原子条件插入**（`UPDLOCK+HOLDLOCK` 的 `WHERE NOT EXISTS`），多副本并发生成只落 1 条，可安全扩到 3 |
| SQL 必须开 TCP 1433 | 默认 SQL Server 只绑回环（`127.0.0.1:1434`），Pod 从宿主机网络访问不到 |
| web 的 nginx 反代到 `rbs-api-svc` | web 与 API 同集群，k8s DNS 直接解析，链路最短、最稳 |
| 连接串用 `host.docker.internal,1433` | Pod 内没有"宿主机网卡"，需靠 hostAliases 把该域名解析到宿主 IP |
| kind 配置里必须有 `extraPortMappings` | Windows/macOS 上 kind **不会自动**把端口映射到宿主机；且同一个 hostPort 只能映射在一个节点上（多节点会端口冲突） |
| 80/443 映射在控制面、30883 映射在 worker-1 | ingress 控制器就调度在控制面（`ingress-ready=true` 标签）；NodePort 兜底在 worker-1 |
| 先部署 API、再部署 web，最后 apply ingress | web 的 nginx 启动时要解析 `rbs-api-svc`；ingress 引用两个 Service，Service 不存在则路由失败 |
| PDF 生成必须跨平台 | 之前 `NotePdf.cs` 用 `System.Drawing`（仅 Windows），在 Linux 容器里 PDF 下载必然报错。**已修复**（见附录 A） |

**涉及文件：**

| 文件 | 状态 | 说明 |
|---|---|---|
| `src/RBS.Api/Dockerfile` | 新建 | API 多阶段镜像，端口 8080，COPY `lib/` |
| `.dockerignore`（仓库根） | 新建 | API 构建上下文是仓库根，排除 web/docs/scripts/k8s/嵌套副本 |
| `k8s/kind-config.yaml` | 修改 | **1 控制面 + 6 worker = 7 节点**；控制面 `ingress-ready=true` + 80/443 映射；worker-1 兜底 30883 映射 |
| `k8s/ingress-nginx.yaml` | 新建 | ingress-nginx 控制器 kind 部署清单（已下载到仓库，离线可用） |
| `k8s/ingress.yaml` | 新建 | 按域名路由：`www.rbsapi.com`(含裸域名)→API，`www.rbsadmin.com`(含裸域名)→web |
| `k8s/api-deployment.yaml` | 新建 | API Deployment（replicas=3 + 连接串 env + hostAliases + 探针） |
| `k8s/api-service.yaml` | 新建 | ClusterIP `rbs-api-svc:8080` |
| `web/nginx.conf` | 修改 | `/api/` 反代 `http://rbs-api-svc:8080`（原指向宿主机 IIS） |
| `k8s/web-deployment.yaml` | 修改 | 移除 hostAliases（不再需要解析 www.rbsapi.com） |
| `k8s/web-service-nodeport.yaml` | 修改 | 说明更新：作为 ingress 的兜底入口（30883） |
| `src/RBS.Infrastructure/PdfGeneration/NotePdf.cs` | **修改（本次修复）** | **移除 System.Drawing**：二维码改 `PngByteQRCode`，条形码改 `PlaceBarcode` 画进 PDF 模板 |
| `src/RBS.Infrastructure/RBS.Infrastructure.csproj` | **修改（本次修复）** | 移除 `System.Drawing.Common` 包引用 |

---

## 阶段 0：软件下载与安装 + 域名解析（每种都提供「终端」和「网址」两种方式）

> 以下命令都在 **Windows PowerShell**（管理员）里执行。已装好的软件可跳过对应行，用最后的验证命令确认即可。

### 0.1 下载清单

| 软件 | 用途 | A. 终端安装（winget，管理员 PowerShell） | B. 网址下载（浏览器直接下载） |
|---|---|---|---|
| WSL2 | 运行 Linux 内核，Docker Desktop 的后端 | `wsl --install`（Windows 11 自带，装完重启） | 官方说明：<https://learn.microsoft.com/zh-cn/windows/wsl/install> |
| Docker Desktop | 容器运行时（kind 节点就运行在它里面） | `winget install --id Docker.DockerDesktop` | <https://www.docker.com/products/docker-desktop/> |
| kind | 用容器搭 Kubernetes 集群 | `winget install --id Kubernetes.kind` | <https://github.com/kubernetes-sigs/kind/releases>（下载 `kind-windows-amd64`） |
| kubectl | 操作集群的命令行工具 | `winget install --id Kubernetes.kubectl` | <https://kubernetes.io/docs/tasks/tools/install-kubectl-windows/>（或直接下 <https://dl.k8s.io/release/v1.36.0/bin/windows/amd64/kubectl.exe>） |
| sqlcmd（可选） | 执行数据库脚本 | `winget install Microsoft.Sqlcmd` | <https://learn.microsoft.com/zh-cn/sql/tools/sqlcmd/sqlcmd-utility> |

**B 方式（网址下载）下载的 exe 怎么用：**

```powershell
# ① 建一个专用工具目录 C:\tools，集中放下载下来的命令行工具
mkdir C:\tools

# ② 把从官网下载的 kind（原名 kind-windows-amd64）改名成 kind.exe 并放进 C:\tools
#    PowerShell 的 Move-Item = 移动（也兼改名）；放进去后系统才能按名字找到 kind 命令
Move-Item ~\Downloads\kind-windows-amd64 C:\tools\kind.exe
# ③ 同上，把下载的 kubectl.exe 也放进来
Move-Item ~\Downloads\kubectl.exe C:\tools\kubectl.exe

# ④ 把 C:\tools 追加进【用户级】PATH 环境变量（永久生效）。
#    说明：$env:Path 是当前会话的临时变量，直接改只对本次终端有效；
#    SetEnvironmentVariable 指定作用域 'User' 才写进注册表永久生效。
#    执行后【重开一个 PowerShell】即可直接用 kind / kubectl 命令。
[Environment]::SetEnvironmentVariable('Path', [Environment]::GetEnvironmentVariable('Path','User') + ';C:\tools', 'User')
```

**安装后验证（四条命令都有输出版本号即成功）：**

```powershell
# ① 确认 WSL 版本为 2（Docker Desktop 的 WSL2 后端依赖它）。
#    Select-Object -First 1 = 只取输出第一行，避免刷屏
wsl --status | Select-Object -First 1
# ② Docker 必须 Client 和 Server 两段都有版本号。
#    只有 Client 没有 Server = 引擎没启动，重启 Docker Desktop 等图标变绿再试
docker version
# ③ kind 有版本号即装好（kind 负责在 Docker 里把 k8s 节点拉成容器）
kind version
# ④ kubectl 有版本号即装好；--client 只校验本地客户端二进制，不连集群
kubectl version --client
```

> ⚠️ Docker Desktop 装完要启动，并确认 Settings → General → **Use the WSL 2 based engine** 已勾选。首次启动会自动装好 WSL2 内核。
>
> ⚠️ 若 `docker version` 只显示 Client 不显示 Server，说明 Docker Desktop 引擎没起来：重启 Docker Desktop，等右下角图标变绿再试。
>
> ⚠️ **内存建议 ≥ 8 GB**（7 节点集群每节点一个 kubelet/systemd 容器 + 镜像构建，内存吃紧会 OOM）。

### 0.2 配置域名解析（hosts 文件，管理员 PowerShell）

两个域名要能访问，必须让它们指向本机 `127.0.0.1`：

```powershell
# ① 用记事本打开 hosts 文件（%WINDIR%\System32\drivers\etc\hosts）。
#    目的是检查是否残留旧的 rbs 域名记录（之前 IIS 部署可能配过），有就删掉避免冲突。
#    注意：必须【管理员身份】运行 PowerShell，否则没权限改 hosts。
notepad.exe "$env:WINDIR\System32\drivers\etc\hosts"

# ② 追加一行域名映射，让两个站点域名都指向本机 127.0.0.1。
#    效果：浏览器访问 www.rbsapi.com / www.rbsadmin.com 时请求发到本机 80 端口，
#    → 经 kind 控制面节点上的 ingress 控制器按 Host 头（请求的域名）路由到对应服务。
#    `n 是换行符；PowerShell 没有 shell 的 echo >> 追加语法，用 Add-Content 追加到文件末尾。
Add-Content -Path "$env:WINDIR\System32\drivers\etc\hosts" -Value "`n127.0.0.1 www.rbsapi.com www.rbsadmin.com"
```

验证：

```powershell
# ping 两个域名，确认都被解析到 127.0.0.1。
# 解析到别的 IP = hosts 没生效或缓存了旧记录（如仍是 IIS 时代的地址），删掉旧行重新加。
ping www.rbsadmin.com
ping www.rbsapi.com
```

> ⚠️ 本机 80/443 当前**空闲**（IIS 未安装/未运行），kind 才能把 80/443 映射到宿主机。若以后装了占用 80 的服务（IIS/其他），会起冲突。

---

## 阶段 1：开放 SQL Server TCP 1433（⚠️ 最大阻塞项）

默认情况下 SQL Server 只监听回环端口，Pod 根本连不上。必须：

1. **打开 SQL Server Configuration Manager**（管理员，或菜单栏搜）：
   - 左侧：`SQL Server 网络配置 → 协议(MSSQLSERVER)`
   - 右侧：双击 **TCP/IP** → 协议选项卡 → **已启用 = 是**
   - **IP 地址**选项卡 → 最下面 **IPAll** → **TCP 端口 = 1433** → **侦听所有 = 是**
   - 确定
2. **重启 SQL Server 服务**：
   ```powershell
   # 重启 SQL Server 服务，让上面改的 TCP/IP 协议配置（启用 + IPAll 端口 1433）生效
   Restart-Service MSSQLSERVER
   ```
3. **防火墙放行**（管理员 PowerShell）：
   ```powershell
   # 放行 Windows 防火墙的入站 TCP 1433：
   # 容器走"宿主机网络"访问 SQL Server 时要过防火墙，不放行会被拦（即使监听正常也连不上）
   New-NetFirewallRule -DisplayName "SQL Server 1433" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
   ```
4. **验证**（必须看到 `0.0.0.0:1433 LISTENING`）：
   ```powershell
   # 验证监听状态：必须看到 0.0.0.0:1433 处于 LISTENING。
   # 若只看到 127.0.0.1:1433 / 127.0.0.1:1434，说明没绑定到所有网卡，Pod 仍然连不上，
   # 回到 Configuration Manager 检查"侦听所有 = 是"并重启服务。
   netstat -ano | findstr :1433
   ```

---

## 阶段 2：创建 7 节点 kind 集群

仓库已带 `k8s/kind-config.yaml`（集群名 `rbs`，**1 控制面 + 6 worker = 7 节点**，节点镜像 `kindest/node:v1.36.1`）：

- **控制面节点**：打了 `ingress-ready=true` 标签（ingress 控制器调度到这），并映射宿主机的 **80/443** 到该节点
- **worker-1**：兜底映射 **30883**（NodePort 直达 web）

```bash
# ① 按 kind-config.yaml 创建名为 rbs 的 7 节点集群（1 控制面 + 6 worker）。
#    节点本质是 Docker 容器，kind 逐个拉起 kubelet/systemd；首次要准备约 1GB 节点镜像，会等几分钟。
#    --config 指定集群配置文件（节点列表、端口映射、污点/标签都写在里面）。
kind create cluster --config k8s/kind-config.yaml

# ② 查看节点状态，期望 7 个都 Ready。
#    有 NotReady = 节点镜像/内存/网络有问题，看阶段 8 排错表最后一行。
kubectl get nodes        # 期望：7 个 Ready（control-plane + 6 个 worker）
```

> 首次创建 7 个节点需要节点镜像 `kindest/node:v1.36.1`（约 1GB，Docker 层缓存复用，只拉一次）。
> **本次实测 Docker Hub（`registry-1.docker.io`）不通**，需先从镜像源拉完再改名，`kind create` 发现本地已有 `kindest/node:v1.36.1` 就会直接复用、不再联网拉：
> ```bash
> # ① 从镜像源 docker.1panel.live 拉取节点镜像（Docker Hub 直连不通时才需要这一步）
> docker pull docker.1panel.live/kindest/node:v1.36.1
> # ② 改名成 kind 期望的原始名 kindest/node:v1.36.1。
> #    kind create 只认这个 tag；本地已有同名镜像就直接复用，不再联网去 registry-1.docker.io 拉
> docker tag docker.1panel.live/kindest/node:v1.36.1 kindest/node:v1.36.1
> ```
>
> ⚠️ 如果集群已存在（比如之前建过），改过 kind-config 后必须**删了重建**才生效：
> ```bash
> # ① 删除旧集群（改过 kind-config 后必须删了重建才生效；--name rbs 指定集群名，与 kind-config 里 name 一致）
> kind delete cluster --name rbs
> # ② 用新的配置重新创建
> kind create cluster --config k8s/kind-config.yaml
> ```

**查宿主机 IP**（关键一步，阶段 5 要用）：

```bash
# 进入 worker 节点容器（docker exec），用 getent hosts 解析 host.docker.internal 这个域名。
# 输出第一列就是宿主机 IP（Docker Desktop WSL2 下通常 192.168.65.254）。
# 这个 IP 要填进 k8s/api-deployment.yaml 的 hostAliases.ip（阶段 5 的 API Pod 靠它连本机 SQL）。
docker exec rbs-worker getent hosts host.docker.internal
```

输出第一列就是宿主 IP（Docker Desktop WSL2 下通常 `192.168.65.254`）。把结果填进 `k8s/api-deployment.yaml` 的 `hostAliases.ip`。

> **兜底**：若上面命令查不到（个别 Docker Desktop 版本 `host.docker.internal` 解析异常），换这两条任选其一：
> ```bash
> # 兜底 1：直接看节点容器的 /etc/hosts，Docker Desktop 会把 host.docker.internal 写进去，grep 出那一行
> docker exec rbs-worker cat /etc/hosts
> # 兜底 2：临时起一个 alpine 容器看【宿主机】的 /etc/hosts（--rm 用完自动删除，不留垃圾容器）
> docker run --rm alpine cat /etc/hosts | grep host.docker.internal
> ```
> 还是不行就直接用默认值 `192.168.65.254`，最后以阶段 7 登录是否成功为准。

---

## 阶段 3：构建并导入镜像

> ⚠️ `kind load docker-image` 会自动把镜像 **加载到全部 7 个节点**（控制面 + 6 个 worker），无需逐个 load。Pod 无论调度到哪个 worker 都能用。（**仅对本地构建的单架构镜像有效**；镜像源拉下来的多平台镜像会失败，见阶段 4 第 2 步。）

**镜像源汇总**（本次实测可用；Docker Hub / registry.k8s.io 直连均不通）：

| 目的 | 镜像源 | 示例 |
|---|---|---|
| Docker Hub 官方镜像（kindest/node、node、nginx…） | `docker.1panel.live` | `docker pull docker.1panel.live/library/node:22-alpine` |
| registry.k8s.io 镜像（ingress-nginx…） | `m.daocloud.io/registry.k8s.io` | `docker pull m.daocloud.io/registry.k8s.io/ingress-nginx/controller:v1.12.1` |
| MCR（.NET SDK/运行时） | `mcr.microsoft.com`（本次直连可达） | `docker pull mcr.microsoft.com/dotnet/sdk:10.0` |

> ⚠️ 不要依赖 `%USERPROFILE%\.docker\daemon.json` 配 registry-mirrors：**Docker Desktop 启动时会覆盖该文件**（本次实测被还原）。一律用「镜像源 `pull` + `docker tag` 改成原名」的方式。

### 3.1 API 镜像（在仓库根目录执行）

```bash
# ① 拉多阶段构建的两个基础镜像：
#    sdk:10.0 → 阶段一，用于编译/发布（含 MSBuild）；aspnet:10.0 → 阶段二，只装运行时、更小。
#    已本地存在可跳过；MCR（mcr.microsoft.com）本次实测直连可达。
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/aspnet:10.0

# ② 构建 API 镜像。
#    ⚠️ 必须在【仓库根目录】执行：-f 指定 Dockerfile 路径，最后的 . 是构建上下文（= 仓库根目录）。
#    因为 RBS.Infrastructure.csproj 用 HintPath 直接引用仓库根 lib/itextsharp.dll、lib/QRCoder.dll，
#    Dockerfile 里 COPY lib/ 依赖这个上下文；上下文不对会报 COPY 失败。
docker build -f src/RBS.Api/Dockerfile -t rbs-api:latest .

# ③ 把镜像导入 kind 集群。
#    kind 节点是独立容器，不共享 Docker Desktop 的镜像仓库，必须 load 进去才拿得到；
#    kind load 会自动把镜像分发到全部 7 个节点，无需逐个 load。
kind load docker-image rbs-api:latest --name rbs

# ④ 逐个节点确认镜像已导入：
#    docker exec 进节点，用 containerd 的命令行工具 ctr 列出镜像，grep 统计命中行数。
#    每行应输出 1（= 该节点有 rbs-api:latest）；输出 0 = 没导上，Pod 调度到该节点会 ImagePullBackOff。
for n in rbs-control-plane rbs-worker rbs-worker2 rbs-worker3 rbs-worker4 rbs-worker5 rbs-worker6; do
  docker exec $n ctr --namespace=k8s.io images list | grep -c "rbs-api:latest"
done
```

- **后备 1**：若 restore 报 itextsharp/QRCoder 相关 NU1101/NU1201 错误（Linux 下 `..\..\lib\` 反斜杠路径异常），把 `src/RBS.Infrastructure/RBS.Infrastructure.csproj` 里两个 HintPath 改为 `/` 分隔（`../../lib/itextsharp.dll`）再构建。
- **后备 2**：若 NuGet restore 超时（国内网络），先配置镜像源再构建：
  ```bash
  # 添加腾讯 NuGet 镜像源并命名为 tencent（restore 超时/下载失败时用）。
  # add source 只是往 NuGet.Config 里写一条源，不改变已有的默认源；加完重新 build 即可。
  dotnet nuget add source https://mirrors.cloud.tencent.com/nuget -n tencent
  ```

### 3.2 Web 镜像（在 web/ 目录执行）

```bash
# ① 拉 web 镜像的两个基础镜像：
#    node:22-alpine → 阶段一，跑 Vite 构建前端产物；nginx:alpine → 阶段二，作为运行时 Web 服务器。
#    Docker Hub 直连不通，所以从镜像源 docker.1panel.live 拉完再 tag 成原名（web/Dockerfile 里引用原名）。
#    && = 前一条成功才执行后一条；同一行内完成「拉 + 改名」两步。
docker pull docker.1panel.live/library/node:22-alpine && docker tag docker.1panel.live/library/node:22-alpine node:22-alpine
docker pull docker.1panel.live/library/nginx:alpine && docker tag docker.1panel.live/library/nginx:alpine nginx:alpine

# ② 进入 web/ 目录构建前端镜像。
#    web/Dockerfile 就在 web/ 下，docker build 默认找当前目录的 Dockerfile，所以不写 -f；
#    -t 给镜像起名 web-web:latest。
cd web
docker build -t web-web:latest .
# ③ 导入 kind 集群（自动分发到 7 个节点）
kind load docker-image web-web:latest --name rbs
# ④ 回到仓库根（后续命令默认在根目录执行，避免路径漂移）
cd ..
```

> ⚠️ **如果本机已经有一个旧的 `web-web:latest`（nginx.conf 改之前构建的），必须重新 build**，否则镜像里 nginx 还在反代 `www.rbsapi.com`（宿主机 IIS），登录会失败。
>
> 不要用 `docker-compose.yml` 那条链路——那是"web 容器 + 宿主机 IIS"的旧方案。

---

## 阶段 4：部署 ingress-nginx 控制器

> 这一步让集群能按域名路由请求。清单已经下载到仓库 `k8s/ingress-nginx.yaml`（离线可用，不用访问 GitHub）。

```bash
# ① 应用 ingress-nginx 控制器清单：
#    一次创建 Deployment/DaemonSet、Service、ConfigMap，以及两个 admission Job（生成 TLS 证书）等所有资源
kubectl apply -f k8s/ingress-nginx.yaml

# ② 阻塞等待控制器 Pod 就绪（最多 180s；首次要从 registry.k8s.io 拉镜像约 30~60 秒）。
#    --namespace 指定命名空间 ingress-nginx；
#    --for=condition=ready 等待 Pod 的 ready 条件成立；
#    --selector 按标签过滤出控制器 Pod（避免等其它 Pod）。
kubectl wait --namespace ingress-nginx --for=condition=ready pod --selector=app.kubernetes.io/component=controller --timeout=180s
# ③ 查看控制器 Pod 状态（期望 Running）。
#    一直 ImagePullBackOff = 国内网络拉不动镜像，走下面后备链路。
kubectl get pods -n ingress-nginx
```

期望：`ingress-nginx-controller-xxxxx` 一个 Running。

> **国内网络拉不动 `registry.k8s.io` 镜像时**（控制器一直 `ImagePullBackOff`）——**实测可行的完整链路**：
>
> **第 1 步：从镜像源拉 + 改标签**（`docker.1panel.live` 等，见阶段 0）：
> ```bash
> # ① 从镜像源 m.daocloud.io/registry.k8s.io 拉取控制器镜像（registry.k8s.io 直连不通时才需要）
> docker pull m.daocloud.io/registry.k8s.io/ingress-nginx/controller:v1.12.1
> # ② 改名成清单里引用的原名 registry.k8s.io/ingress-nginx/controller:v1.12.1（kubelet 按这个名字找镜像）
> docker tag m.daocloud.io/registry.k8s.io/ingress-nginx/controller:v1.12.1 registry.k8s.io/ingress-nginx/controller:v1.12.1
> # ③ 同上：拉 + 改名 admission 用的证书生成器镜像 kube-webhook-certgen（两个 Job 要用）
> docker pull m.daocloud.io/registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2
> docker tag m.daocloud.io/registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2 registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2
> ```
>
> **第 2 步：** ❌ **不要用 `kind load docker-image`** —— 镜像源拉下来的镜像带多平台索引，kind 的 `ctr import --all-platforms` 会报 `content digest ... not found`。
> 改用**管道直接导入节点 containerd**（要哪些节点就逐个执行，admission Job 落点不定，建议 7 个节点都导）：
> ```bash
> # 对 7 个节点逐个导入。为什么不用 kind load：镜像源拉的是【多平台索引】镜像，kind 导入会报
> # "content digest ... not found"。这里改用管道：
> #   docker save 把本机镜像导出成镜像流（保留当前 linux/amd64 架构的单个镜像），
> #   经 | 管道喂给节点里的 containerd（ctr images import），绕开 kind 的多平台处理。
> #   -i 让 docker exec 把 stdin（镜像流）送进节点；--platform linux/amd64 明确指定架构；
> #   末位 - 表示从 stdin 读入。admission Job 落点不定，所以 7 个节点都导。
> for n in rbs-control-plane rbs-worker rbs-worker2 rbs-worker3 rbs-worker4 rbs-worker5 rbs-worker6; do
>   docker save registry.k8s.io/ingress-nginx/controller:v1.12.1 | docker exec -i $n ctr --namespace=k8s.io images import --platform linux/amd64 -
>   docker save registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2 | docker exec -i $n ctr --namespace=k8s.io images import --platform linux/amd64 -
> done
> ```
>
> **第 3 步：摘掉 `k8s/ingress-nginx.yaml` 里的 `@sha256:...` digest 引用**。
> 仓库里这份清单已去掉（3 处 image 都只写 `repo:tag`）。原因：镜像源镜像的顶层 digest 可能与上游不同，且 kubelet 对 digest 固定引用会**先向 registry 发 HEAD 确认**（网络不通直接失败），摘掉后按 tag 命中本地即可。若你是从上游重新下载的清单，记得手动删掉 `@sha256:xxxx`。
>
> **第 4 步：重建相关 Pod / Job**（Job 的 `spec.template` 不可变，改了镜像必须 `kubectl delete job` 后重新 apply）：
> ```bash
> # ① 重新 apply，把改过镜像引用的清单下发（后面几步都是配合"改镜像后重建"）
> kubectl apply -f k8s/ingress-nginx.yaml
> # ② 删掉旧控制器 Pod：镜像 tag 没变时 apply 不会自动重建，必须手动删让控制器按新镜像重新创建
> kubectl delete pod -n ingress-nginx -l app.kubernetes.io/name=ingress-nginx
> # ③ 删掉两个 admission Job —— 必须删：Job 的 spec.template 不可变，改了镜像直接 apply 会报
> #    "field is immutable"；删掉后由控制器按新清单重新创建并运行。
> kubectl delete job -n ingress-nginx ingress-nginx-admission-create ingress-nginx-admission-patch
> # ④ 重新 apply，让控制器重建上面删掉的 Pod / Job
> kubectl apply -f k8s/ingress-nginx.yaml
> ```
>
> **第 5 步：验证**：
> ```bash
> # ① 再等控制器就绪（新镜像拉取/启动约 30~60 秒）
> kubectl wait --namespace ingress-nginx --for=condition=ready pod --selector=app.kubernetes.io/component=controller --timeout=180s
> # ② 同时看 Pod 和 Job：期望 controller Running、两个 admission Job Complete
> kubectl get pods,jobs -n ingress-nginx
> ```
> 期望：controller `Running`、两个 admission Job `Complete`、宿主机 `curl http://localhost/` 返回 404（ingress 默认后端）。
> 若 controller 一直 `ContainerCreating` 且报 `secret "ingress-nginx-admission" not found`，是 admission Job 没跑完（先解决上面第 2~4 步）。

---

## 阶段 5：部署 API（先部署 API，再部署 web）

```bash
# ① 创建 API 的 Deployment（replicas=3 + 连接串 env + hostAliases + 探针，见 k8s/api-deployment.yaml）
kubectl apply -f k8s/api-deployment.yaml
# ② 创建 ClusterIP Service rbs-api-svc:8080（web 的反代和 ingress 路由都靠它做负载均衡）
kubectl apply -f k8s/api-service.yaml

# ③ 阻塞等待 API Pod Ready（最多 120s）。
#    readiness 探针会请求 /swagger/v1/swagger.json，返回 200 才标记 1/1 Ready；
#    一直不 Ready = 镜像没导全或探针没通，看阶段 8 排错表。
kubectl wait --for=condition=ready pod -l app=rbs-api --timeout=120s
# ④ -o wide 多显示一列 NODE，可确认 3 个副本确实分布在 3 个不同 worker 节点
kubectl get pods -l app=rbs-api -o wide
```

**验证：**

```bash
# ① 再确认一次状态：Running 且 1/1 Ready 才算就绪
kubectl get pods -l app=rbs-api

# ② 看启动日志（Serilog 同时写 Console + 文件，kubectl logs 有内容）。
#    出现"调度引擎启动 / Job AutoRenewJob/... 执行成功"说明 SQL 已连通（调度任务要查库才能成功）；
#    否则重点看有没有连接串 / SQL 相关报错。
kubectl logs -l app=rbs-api --tail=100
```

> **注意**：`aspnet:10.0` 基础镜像**不带 curl**，不要在 Pod 里 `kubectl exec ... curl`。用探针状态 + 阶段 7 的域名访问验证即可。
>
> Pod `Running` + `Ready` 只代表 API 起来了。SQL 连不连得上，看日志里调度任务是否"执行成功"可**初步确认**（本次实测日志有 `调度引擎启动`、`到期排期: N 条`、`Job AutoRenewJob/... 执行成功`——调度任务要查库，能成功说明 SQL 通了）；最终以阶段 7 登录为准。

---

## 阶段 6：部署 web

```bash
# ① 创建 web 的 Deployment（replicas=3，nginx 托管静态页 + /api 反代到 rbs-api-svc）
kubectl apply -f k8s/web-deployment.yaml
# ② 创建 NodePort Service：集群内 80 端口 + 宿主机 30883 端口两个入口（30883 就是兜底入口）
kubectl apply -f k8s/web-service-nodeport.yaml

# ③ 查看 3 个 web Pod 的状态与分布（-o wide 的 NODE 列；期望 3 个 Running、分别在 3 个不同 worker）
kubectl get pods -l app=rbs-web -o wide
```

> `web-service.yaml`（LoadBalancer 30882）是给 Docker Desktop 内置集群用的，kind 里没有 LoadBalancer 提供方，别用它。

---

## 阶段 7：应用 Ingress 并端到端验证

### 7.1 应用 Ingress

```bash
# ① 应用 Ingress：声明按域名路由的规则
#    （www.rbsapi.com → rbs-api-svc:8080，www.rbsadmin.com → rbs-web-svc:80）
kubectl apply -f k8s/ingress.yaml
# ② 查看 Ingress；ADDRESS 列有值 = 已绑定到 ingress 控制器，可以按域名访问。
#    ADDRESS 空 = 控制器没就绪（阶段 4 没做完）。
kubectl get ingress
```

### 7.2 验证 api 站点（www.rbsapi.com）

```powershell
# ① 检查 API 元数据端点是否可达（不显示响应体，只输出状态码和 Content-Type）。
#    -s 静默（隐藏进度条）；-o /dev/null 丢弃响应体；-w 自定义输出 HTTP 状态码 + 响应类型。
#    期望：HTTP 200  application/json；非 200 = ingress 路由没通或 API 没起来。
curl.exe -s -o /dev/null -w "HTTP %{http_code}  %{content_type}\n" http://www.rbsapi.com/swagger/v1/swagger.json

# ② 直接调登录接口（POST + JSON body）——一步同时验证 ingress 路由、API、SQL 三层。
#    -X POST 指定方法；-H 加 Content-Type 请求头声明发送 JSON；-d 传登录体。
#    期望：返回 { token, user, roles, permissions }；
#    401 = API 通但账号密码不对；500 = SQL 或 API 有问题（看 kubectl logs）。
curl.exe -s -X POST http://www.rbsapi.com/api/Auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"123456"}'
```

浏览器打开 **`http://www.rbsapi.com/swagger`** 应能看到 Swagger UI（API 站点）。

### 7.3 验证 web 站点（www.rbsadmin.com）

浏览器打开 **`http://www.rbsadmin.com`** → 用 **admin / 123456** 登录。

登录成功即全链路通：
`www.rbsadmin.com → 127.0.0.1:80 → ingress → rbs-web-svc → web Pod(nginx) → rbs-api-svc → RBS.Api Pod → hostAliases → SQL(1433)`

**验证 PDF 下载（这是之前阻塞容器化的设计问题，见附录 A）。** 下面按本次实测流程，直接走 API 域名 `www.rbsapi.com` 取 token 再下载（Git Bash 语法；走 web 域名的 `www.rbsadmin.com/api/...` 同样可行）：

```bash
# ① 先登录拿 token：调用登录接口，从返回的 JSON 里用 grep 抓出 "token":"..." 这一段，
#    （-oE '"token":"[^"]+"' 匹配 token 字段整段），再用 cut 按双引号切分取第 4 段（token 的值），
#    存进变量 TOKEN。之后带鉴权的接口都要在请求头里带 Authorization: Bearer <token>。
TOKEN=$(curl -s -X POST http://www.rbsapi.com/api/Auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"123456"}' | grep -oE '"token":"[^"]+"' | cut -d'"' -f4)

# ② 下载缴款通知书 PDF：
#    -H "Authorization: Bearer $TOKEN" 携带鉴权头（下载接口需要登录态）；
#    -o dn_test.pdf 把响应体写成本地文件。
curl -s -H "Authorization: Bearer $TOKEN" http://www.rbsapi.com/api/Note/dndownload -o dn_test.pdf
#    file 校验文件头，期望输出 "PDF document, version 1.4, 2 page(s)"。
#    若显示 "HTML document" = 接口 500/404 返回了错误页；"data"/JSON 文本 = 接口返回了错误对象。
file dn_test.pdf

# ③ 下载催缴通知书 PDF（rndownload，含条形码），同样 -o 存文件再 file 校验
curl -s -H "Authorization: Bearer $TOKEN" http://www.rbsapi.com/api/Note/rndownload -o rn_test.pdf
file rn_test.pdf            # 期望同上，约 53~55 KB
```

期望输出：`application/pdf`、文件头 `%PDF`、约 53~55 KB、2 页，双击能正常打开且**二维码、条形码、中文都正常显示**。本次实测两个 PDF 已存档在仓库 `k8s/verify/` 可打开核对。

> PowerShell 里等价写法：`$token = (curl.exe -s -X POST http://www.rbsapi.com/api/Auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"123456"}' | ConvertFrom-Json).token`，再用 `curl.exe -H "Authorization: Bearer $token"` 下载。
>
> 同理可测 `dnpreview` / `rnpreview`（预览，浏览器内联显示）。
>
> 若返回 500：先看 API 日志——`kubectl logs -l app=rbs-api --tail=100`，重点看是否抛异常（修复后不应再有 `PlatformNotSupportedException` / `System.Drawing` 相关错误）。

### 7.4 登录失败时按链路逐段排查

```bash
# ① 页面能开但登录报网络错误 → 绕过 web 直接直连 API 登录，判断 API 层通不通。
#    （前面 7.2 的 curl 登录已覆盖这一步；API 通、SQL 通、账号对才返回 token）
curl.exe -s -X POST http://www.rbsapi.com/api/auth/login -H "Content-Type: application/json" -d '{"username":"admin","password":"123456"}'

# ② API 域名通、但 web 登录不通 → 说明问题出在 web→API 的反代段。
#    进入 web Pod 里用 wget 直连集群内 Service 地址 rbs-api-svc:8080（nginx:alpine 自带 busybox wget）。
#    kubectl exec = 在指定容器里执行命令；-- 后面是容器内要跑的命令；
#    wget -qO- 把响应打到 stdout；| head -c 200 只取前 200 字符看个开头。
#    有输出 = Service 解析与路由正常；无输出 = rbs-api-svc 不存在或 web 用的 nginx.conf 是旧的（指向 IIS）。
kubectl exec deploy/rbs-web -- wget -qO- http://rbs-api-svc:8080/swagger/v1/swagger.json | head -c 200

# ③ 域名打不开（www.rbsadmin.com 超时/404）→ 按 域名解析 → Ingress → 控制器 → 兜底入口 逐层查：
ping www.rbsadmin.com                                    # 必须解析到 127.0.0.1（不是就是 hosts 没生效）
kubectl get ingress                                      # 两个 Ingress 的 ADDRESS 列必须有值
kubectl get pods -n ingress-nginx                        # 控制器 Pod 必须 Running
curl.exe -s -o /dev/null -w "%{http_code}\n" http://localhost:30883   # 兜底入口通 = web 本身是好的，问题在 ingress/hosts

# ④ API 起不来 / 接口 500 → 看日志定位（Serilog 同时写 Console 和文件，kubectl logs 有内容）
kubectl logs -l app=rbs-api --tail=100
```

---

## 阶段 8：日常操作

### 改代码后重新发布

```bash
# ── API：改过后端代码后发布 ──
docker build -f src/RBS.Api/Dockerfile -t rbs-api:latest .   # ① 重新构建镜像（tag 仍是 latest）
kind load docker-image rbs-api:latest --name rbs              # ② 导入集群（自动分发到 7 节点）
kubectl rollout restart deployment rbs-api                    # ③ 滚动重启：tag 没变，apply 不会重建 Pod，必须 restart 触发

# ── Web：改了前端代码或 nginx.conf 都要重新 build ──
cd web && docker build -t web-web:latest . && cd ..           # ① 重新构建（web/Dockerfile 在 web/ 下）
kind load docker-image web-web:latest --name rbs              # ② 导入集群
kubectl rollout restart deployment rbs-web                    # ③ 滚动重启（原因同上）
```

### 排错表

| 现象 | 排查 |
|---|---|
| `ImagePullBackOff` / `ErrImagePull` | 镜像没 load 进集群：重新 `kind load docker-image`（会加载到全部 7 节点）；ingress 相关镜像拉不动见阶段 4 后备 |
| Pod 一直 `Pending` / `CrashLoopBackOff` | `kubectl describe pod <名字>`、`kubectl logs -l app=<名字> --tail=100` |
| API Pod `Running` 但 `0/1 Ready` | Swagger 探针没通：`kubectl get events` 或 `kubectl describe pod <名字>`；确认 api-service 已创建 |
| web Pod 起不来、报 `host not found in upstream` | `rbs-api-svc` 不存在或晚于 web 创建：先 apply api-service，再重启 web |
| `www.rbsadmin.com` / `www.rbsapi.com` 打不开 | ① hosts 是否配好（`ping` 必须 127.0.0.1）；② kind-config 是否带 80/443 的 extraPortMappings 且重建过集群；③ ingress 控制器是否 Running（阶段 4）；④ `kubectl get ingress` ADDRESS 有值；⑤ 用 `http://localhost:30883` 兜底确认 web 本身是好的 |
| 浏览器报 404 / 502 | 404=host 没匹配上（域名没写对或 hosts 没配）；502=后端 Service 没就绪（`kubectl get pods` 看 web/api 是否 Running） |
| 页面能开、登录报网络错误 | 按阶段 7.4 ① → ② 逐段测；重点看 `kubectl logs` 是否 SQL 连接失败 |
| 连接串报错 / SQL 连不上 | ① 阶段 1 是否出现 `0.0.0.0:1433 LISTENING`；② hostAliases IP 是否填对（阶段 2 查到或兜底值）；③ 防火墙 1433 是否放行 |
| 登录成功但 PDF 预览/下载报错 | 见附录 A。先看 API 日志是否有异常；确认用的是修复后的镜像（`NotePdf.cs` 已无 System.Drawing） |
| 时间/日志时区不对 | 业务时间用 `ChinaTime.Now`（显式 UTC+8，与容器时区无关）；只有 Serilog 日志时间戳跟随容器时区（默认 UTC）。想改成北京时间就在 deployment 里加 `TZ=Asia/Shanghai` 环境变量，不影响业务逻辑 |
| 调度任务没按时跑 | 确认 `kubectl get pods -l app=rbs-api` 副本都在 Running（现为 3 副本，生成侧已原子条件插入，多副本不会再重复生成） |
| API Pod 反复重启、`kubectl get pods` 显示 OOMKilled | 内存上限太小：调大 `api-deployment.yaml` 的 `limits.memory`（或降低 `Scheduling:PdfParallelism`），改完重新 `kubectl apply -f k8s/api-deployment.yaml` |
| 大报表/导出/批量出账请求返回 504 网关超时 | nginx 反代读超时 60s 偏短：把 `web/nginx.conf` 的 `proxy_read_timeout` 调大（如 300s），重建 web 镜像并 `kubectl rollout restart deployment rbs-web` |
| 7 节点集群创建后部分节点 `NotReady` | `docker logs <kind-node容器>` 看 kubelet 报错；内存不足时优先减 worker 数量（如 1 控制面 + 3 worker）或给 Docker Desktop 加内存 |

### 清理

```bash
# ① 删除 Ingress 路由与 ingress-nginx 控制器（先拆掉入口，流量先停）
kubectl delete -f k8s/ingress.yaml -f k8s/ingress-nginx.yaml
# ② 删除 API/Web 的 Deployment 和 Service（Pod 随之被回收，集群里不剩业务资源）
kubectl delete -f k8s/api-deployment.yaml -f k8s/api-service.yaml -f k8s/web-deployment.yaml -f k8s/web-service-nodeport.yaml
# ③ 整个删掉 kind 集群（7 个节点容器一起清理；不会删本机 Docker 镜像，想重来随时 recreate）
kind delete cluster --name rbs
# 可选：把 hosts 里加的两行域名解析删掉
```

---

## 附录 A：PDF 下载设计问题——已确认存在并已修复 ✅

### A.1 问题是什么（为什么之前"不能 docker 部署"）

`src/RBS.Infrastructure/PdfGeneration/NotePdf.cs` 的 PDF 生成用到了 **System.Drawing**，而 System.Drawing 只在 Windows 上有原生 GDI+ 实现，**Linux 容器里调用必然抛 `PlatformNotSupportedException`**：

| 原实现 | 位置 | 问题 |
|---|---|---|
| 文件头 `#pragma warning disable CA1416 // System.Drawing 仅 Windows 支持，本应用仅在 Windows 部署` | 第 1 行 | 直接声明了"仅 Windows"假设 |
| `using System.Drawing; using System.Drawing.Imaging;` | 第 3-4 行 | Linux 上这些类型可用但调用即抛 |
| 二维码：`new QRCode(...).GetGraphic(20)` + `new Bitmap(...)` + `Bitmap.Save(Gif)` | `GetPaymentQRCodeByte` | `GetGraphic` 和 `Bitmap` 都是 System.Drawing，Linux 上抛异常 |
| 条形码：`code128.CreateDrawingImage(...)` + `Bitmap` + GIF | `GenBatCode` | `CreateDrawingImage` 是 System.Drawing 方案，且此方法**没有 try/catch**，一遇到条形码就硬崩 |

而 `NoteController` 的预览/下载接口（`dnpreview` / `dndownload` / `rnpreview` / `rndownload`）的 Mock 数据**固定带 Barcode**，所以只要容器化部署，这 4 个接口一调用就 500 → **PDF 下载功能在 Docker 里完全不可用**。这就是"下载 API 下载 PDF 存在设计问题，导致不能使用 docker 部署"的根源。

### A.2 修复内容（本次已改，均已编译验证）

1. **二维码** → 改用同库 `PngByteQRCode`（纯托管，直接输出 PNG）：
   - `new PngByteQRCode(qrCodeData)` → `qrCode.GetGraphic(20)` 返回 byte[]，**不经过 System.Drawing**
2. **条形码** → 改用 iTextSharp 直接画进 PDF 模板（矢量，跨平台）：
   - `writer.DirectContent.CreateTemplate(...)` 建模板 → `code128.PlaceBarcode(template, BaseColor.BLACK, BaseColor.WHITE)` 画条码 → `Image.GetInstance(template)` 转图片
   - 先用量一次实际尺寸再按尺寸生成模板，保持与原 `CreateDrawingImage` 相同的几何；加 try/catch，失败返回 null（调用点已 null 守卫，条码缺失不影响整张 PDF）
3. **文件头** → 删除 `#pragma warning disable CA1416` 和两个 `using System.Drawing.*`
4. **包引用** → 移除 `RBS.Infrastructure.csproj` 里已无人调用的 `System.Drawing.Common`
5. **字体** → 中文用 `Content/Fonts/SimHei.ttf`，由 iTextSharp 纯托管加载（Linux 上无需 libgdiplus），已随发布产物拷贝，路径大小写与 csproj 一致

### A.3 为什么修复后一定能在 Linux 上跑

- 代码里已**没有任何 System.Drawing 调用**（全局 grep 只剩注释里的字样）
- iTextSharp（条形码、PDF 模板）和 QRCoder 的 `PngByteQRCode` 都是**纯 C# 托管实现**，不依赖 GDI+/libgdiplus
- `RBS.Api.csproj` 的字体、SqlMaps.xml 等都由 `CopyToOutputDirectory` 自动带入发布产物，容器里路径和本机一致

### A.4 部署后怎么验证（对应阶段 7）

按阶段 7.3 的 curl 流程下载 PDF：返回 `200 application/pdf`、文件头 `%PDF`、能打开、二维码/条形码/中文正常。若历史镜像还是旧的，请**重新 build API 镜像**再部署（阶段 8 的发布命令），别复用之前构建的 `rbs-api:latest`。

---

## 注意事项

- 仓库根下的 `RentalBillingSystem/` 是**内嵌的旧重复副本**，所有改动只针对仓库根的 `src/`、`web/`、`k8s/`，别改错位置。
- **访问方式**：主入口是域名（`www.rbsadmin.com` / `www.rbsapi.com`，无端口）；兜底是 `http://localhost:30883`（NodePort 直达 web）。用 IP 或 `localhost:80` 访问不会命中 ingress 路由，会 404。
- **时区**：`ChinaTime` 用显式 UTC+8（`ChinaTime.cs`），业务不受容器时区影响；.NET 10 on Linux 支持 Windows 时区 ID（`China Standard Time` → 自动映射 `Asia/Shanghai`），镜像自带 tzdata/ICU，不会因时区 ID 崩溃。
- **探针**：liveness/readiness 用 `/swagger/v1/swagger.json`（匿名 GET）。前提是 appsettings.json 的 `Swagger:Enabled=true`（Production 下也开启）；若你以后把它关了，探针会失败、Pod 会被重启，届时需换成其它匿名端点。
- 数据库脚本执行方式不变（sqlcmd 连 `Server=localhost` 或本机 `Server=.`）。

---

## 附录 B：完整命令序列（按本次实测执行顺序）

> 按 1→N 逐条执行即可复现本次部署。**前置**：阶段 0 软件已装、hosts 已配（`127.0.0.1 www.rbsapi.com www.rbsadmin.com`）、阶段 1 SQL 1433 已开放且 `netstat -ano | findstr :1433` 显示 `0.0.0.0:1433 LISTENING`。
> 本序列默认在**仓库根目录**执行（Git Bash）；涉及 `cd web` 的命令已标注。
> **每条命令的详细作用解释见对应阶段**：①→阶段 2，②→阶段 3.1，③→阶段 3.2，④→阶段 4，⑤→阶段 5，⑥→阶段 6，⑦→阶段 7。这里是可整段复制的精简版。

```bash
# ════════════════ ① 创建 7 节点 kind 集群 ════════════════
# 节点镜像走镜像源（Docker Hub 直连不通）：拉完改名成 kind 期望的原始名
docker pull docker.1panel.live/kindest/node:v1.36.1
docker tag docker.1panel.live/kindest/node:v1.36.1 kindest/node:v1.36.1
# 按 kind-config.yaml 创建集群（1 控制面 + 6 worker，控制面映射 80/443、worker-1 映射 30883）
kind create cluster --config k8s/kind-config.yaml
kubectl get nodes                 # 期望：rbs-control-plane + rbs-worker~worker6 共 7 个 Ready

# 查宿主机 IP（getent 解析 host.docker.internal）→ 填进 k8s/api-deployment.yaml 的 hostAliases.ip
#（本次实测 192.168.65.254；API Pod 靠它连本机 SQL Server）
docker exec rbs-worker getent hosts host.docker.internal

# ════════════════ ② 构建 API 镜像 ════════════════
# 拉多阶段构建基础镜像：sdk=编译，aspnet=运行时
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/aspnet:10.0
# 在【仓库根目录】构建（-f 指定 Dockerfile，. 是构建上下文；lib/ 依赖根目录）
docker build -f src/RBS.Api/Dockerfile -t rbs-api:latest .
# 导入 kind 集群（自动分发到 7 个节点）
kind load docker-image rbs-api:latest --name rbs

# ════════════════ ③ 构建 Web 镜像 ════════════════
# 拉 node 构建镜像 + nginx 运行镜像（镜像源拉完改名成 Dockerfile 引用的原名）
docker pull docker.1panel.live/library/node:22-alpine && docker tag docker.1panel.live/library/node:22-alpine node:22-alpine
docker pull docker.1panel.live/library/nginx:alpine && docker tag docker.1panel.live/library/nginx:alpine nginx:alpine
# 进 web/ 构建前端镜像（web/Dockerfile 在 web/ 下），再回根目录
cd web && docker build -t web-web:latest . && cd ..
kind load docker-image web-web:latest --name rbs

# ════════════════ ④ 部署 ingress-nginx ════════════════
# 从镜像源拉控制器镜像并改名成清单引用名（registry.k8s.io 直连不通）
docker pull m.daocloud.io/registry.k8s.io/ingress-nginx/controller:v1.12.1
docker tag m.daocloud.io/registry.k8s.io/ingress-nginx/controller:v1.12.1 registry.k8s.io/ingress-nginx/controller:v1.12.1
# 同上：拉 + 改名 admission 证书生成器镜像（两个 Job 用）
docker pull m.daocloud.io/registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2
docker tag m.daocloud.io/registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2 registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2

# 管道导入 7 个节点（多平台镜像不能用 kind load）：
# docker save 导出镜像流 → 管道喂给节点 containerd 的 ctr import，逐个节点执行
for n in rbs-control-plane rbs-worker rbs-worker2 rbs-worker3 rbs-worker4 rbs-worker5 rbs-worker6; do
  docker save registry.k8s.io/ingress-nginx/controller:v1.12.1 | docker exec -i $n ctr --namespace=k8s.io images import --platform linux/amd64 -
  docker save registry.k8s.io/ingress-nginx/kube-webhook-certgen:v1.5.2 | docker exec -i $n ctr --namespace=k8s.io images import --platform linux/amd64 -
done

# 应用清单（仓库版已去掉 @sha256 digest；kubelet 对 digest 固定引用会先发 HEAD，网络不通必失败）。
# 删旧 Pod/Job 再 apply：tag 没变 apply 不重建，且 Job 的 spec.template 不可变
kubectl apply -f k8s/ingress-nginx.yaml
kubectl delete pods -n ingress-nginx --all
kubectl delete job -n ingress-nginx ingress-nginx-admission-create ingress-nginx-admission-patch
kubectl apply -f k8s/ingress-nginx.yaml
# 等控制器 Ready，再查状态
kubectl wait --namespace ingress-nginx --for=condition=ready pod --selector=app.kubernetes.io/component=controller --timeout=180s
kubectl get pods,jobs -n ingress-nginx   # 期望：controller Running，两个 admission Job Complete

# ════════════════ ⑤ 部署 API（先 API 再 web，最后 ingress） ════════════════
# 创建 API Deployment + ClusterIP Service rbs-api-svc:8080
kubectl apply -f k8s/api-deployment.yaml -f k8s/api-service.yaml
# 等 3 个副本 Ready（readiness 探针打 /swagger/v1/swagger.json）
kubectl wait --for=condition=ready pod -l app=rbs-api --timeout=120s
kubectl logs -l app=rbs-api --tail=30    # 出现"调度引擎启动 / Job ... 执行成功"说明 SQL 已连通

# ════════════════ ⑥ 部署 Web ════════════════
# 创建 web Deployment + NodePort Service（宿主机 30883 兜底入口）
kubectl apply -f k8s/web-deployment.yaml -f k8s/web-service-nodeport.yaml
kubectl get pods -l app=rbs-web -o wide  # 期望：3 个 Running，分散在不同 worker

# ════════════════ ⑦ 应用 Ingress 并端到端验证 ════════════════
# 应用按域名路由规则（www.rbsapi.com→API，www.rbsadmin.com→web）
kubectl apply -f k8s/ingress.yaml
kubectl get ingress                      # 期望 rbs-api-ingress / rbs-web-ingress 的 ADDRESS 列有值

# ① API 站点：只输出状态码验证元数据端点；再调登录接口验证 ingress+API+SQL 三层
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://www.rbsapi.com/swagger/v1/swagger.json        # 期望 200
curl -s -X POST http://www.rbsapi.com/api/Auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"123456"}'   # 期望返回 token

# ② Web 站点（含 web→API 反代链路）：首页 200 + 经 web 登录返回 token
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://www.rbsadmin.com/                              # 期望 200
curl -s -X POST http://www.rbsadmin.com/api/Auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"123456"}'  # 期望返回 token

# ③ 兜底入口：NodePort 30883 直达 web
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://localhost:30883                               # 期望 200

# ④ PDF 下载（验证附录 A 的跨平台修复）：
#    登录拿 token → 带 Authorization 头下载两个通知书 PDF → file 校验文件头
TOKEN=$(curl -s -X POST http://www.rbsapi.com/api/Auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"123456"}' | grep -oE '"token":"[^"]+"' | cut -d'"' -f4)
curl -s -H "Authorization: Bearer $TOKEN" http://www.rbsapi.com/api/Note/dndownload -o dn_test.pdf && file dn_test.pdf   # 期望 PDF, 2 pages
curl -s -H "Authorization: Bearer $TOKEN" http://www.rbsapi.com/api/Note/rndownload -o rn_test.pdf && file rn_test.pdf   # 期望 PDF, 2 pages
```

> 以后发布新版本：重新 `docker build` + `kind load docker-image` 后执行 `kubectl rollout restart deployment/rbs-api`（或 `rbs-web`），见阶段 8。
