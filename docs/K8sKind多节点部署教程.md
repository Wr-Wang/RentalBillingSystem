# Docker Desktop + kind 多节点部署前端教程（以 rbs-web 为例）

> 从 Docker 单容器升级到 **Kubernetes（K8s）多节点**：用 **kind**（Kubernetes IN Docker）在 Docker Desktop 里搭一个 1 个控制平面 + 2 个工作节点的集群，把前端 `rbs-web` 部署进去。
>
> **本机现状（已探测）**：Docker ✅、kubectl ✅、kind ❌（本篇含安装）、winget ✅。
>
> **学习收益**：一次搞定 K8s 核心概念——节点 / Pod / Deployment / Service(NodePort) / 滚动更新 / 自愈 / 扩缩容，且**随时可删、不留痕迹**。

---

## 目录

- [第 0 章 先理解：kind 是什么、为什么用它](#第-0-章-先理解kind-是什么为什么用它)
- [第 1 章 安装 kind（本机缺的）](#第-1-章-安装-kind本机缺的)
- [第 2 章 创建多节点集群](#第-2-章-创建多节点集群)
- [第 3 章 认识你的集群](#第-3-章-认识你的集群)
- [第 4 章 构建镜像并导入集群](#第-4-章-构建镜像并导入集群)
- [第 5 章 编写部署清单（Deployment + Service）](#第-5-章-编写部署清单deployment--service)
- [第 6 章 部署并访问前端](#第-6-章-部署并访问前端)
- [第 7 章 打通后端 /api（hostAliases 详解）](#第-7-章-打通后端-apihostaliases-详解)
- [第 8 章 生产操作：扩缩容 / 滚动更新 / 自愈](#第-8-章-生产操作扩缩容--滚动更新--自愈)
- [第 9 章 常用命令速查](#第-9-章-常用命令速查)
- [第 10 章 排错表](#第-10-章-排错表)
- [第 11 章 清理](#第-11-章-清理)
- [附录 A K8s 面试要点（速记版）](#附录-a-k8s-面试要点速记版)

---

## 第 0 章 先理解：kind 是什么、为什么用它

### 0.1 kind = Kubernetes IN Docker

kind 用 **Docker 容器来充当 K8s 的节点**。每个节点（control-plane / worker）其实就是一个容器，容器里跑着真实的 K8s 组件（kubelet、containerd 等）。

```
┌─────────────── 你的 Windows ─────────────────────────────┐
│  Docker Desktop（WSL2 后端）                               │
│                                                          │
│  kind 创建的一堆容器（= K8s 节点）                          │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐ │
│  │ rbs-control-plane │ rbs-worker  │   │ rbs-worker2  │ │
│  │ (api-server 等)    │  ┌───────┐  │   │  ┌───────┐  │ │
│  │                    │  │ Pod   │  │   │  │ Pod   │  │ │
│  │                    │  └───────┘  │   │  └───────┘  │ │
│  └──────────────┘   └──────────────┘   └──────────────┘ │
│        ▲ kubectl（本机已装）                                │
└──────────────────────────────────────────────────────────┘
```

### 0.2 为什么用 kind，不用 Docker Desktop 内置 K8s

| | kind | Docker Desktop 内置 K8s |
|---|---|---|
| 本质 | Docker 容器当节点，**独立于 Docker Desktop** | Docker Desktop 自带的一键集群 |
| 多节点 | **天然支持**（配置里写几个就几个） | 默认单节点，多节点麻烦 |
| 卸载 | `kind delete cluster` 干干净净 | 要在 Docker Desktop 里关 |
| 环境隔离 | 可建多个集群互不干扰 | 只能一个 |
| 学习/CI | 社区标准、CI 里也用 | 桌面专用 |

**结论**：学 K8s、模拟多节点、验证清单，用 kind 是最佳选择。

### 0.3 需要理解的核心概念（本教程会用到）

| 概念 | 一句话 | 对应 Docker |
|---|---|---|
| **Pod** | K8s 最小调度单位（1 个或多个容器） | ≈ 容器 |
| **Deployment** | 声明"我要几个副本、镜像是什么"，负责创建/更新/自愈 Pod | ≈ compose 的 service |
| **Service** | 给一组 Pod 提供稳定访问入口（负载均衡） | ≈ compose 端口映射 + DNS |
| **NodePort** | Service 的一种，把端口暴露到每个节点的固定端口（30000-32767） | ≈ `-p 宿主机:容器` |
| **hostAliases** | 往 Pod 的 /etc/hosts 写域名→IP | ≈ compose 的 extra_hosts |
| **Node / Taint** | 节点是机器；control-plane 默认有污点，普通 Pod 不会调度上去 | ≈ 宿主机 |

---

## 第 1 章 安装 kind（本机缺的）

打开 **PowerShell**（非管理员即可），执行：

```powershell
winget install --id Kubernetes.kind
```

安装完成后，**重新打开一个 PowerShell 窗口**（让 PATH 生效），验证：

```powershell
kind version
```

**期望输出**：

```
kind v0.26.0  go1.23.x linux/amd64
```

> 如果 winget 找不到，备选：从 https://kind.sigs.k8s.io 下载 `kind-windows-amd64.exe`，改名 `kind.exe` 放到 `C:\Windows\System32\`（或任意已在 PATH 的目录）。

> kubectl 本机已装（v1.36.1），跳过。若换机器：`winget install -e --id Kubernetes.kubectl`。

---

## 第 2 章 创建多节点集群

### 2.1 写集群配置文件

在工作目录建一个 `kind-config.yaml`（放哪都行，比如 `C:\Users\Administrator\kind-config.yaml`）：

```yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
name: rbs                                  # 集群名（下面命令统一用 --name rbs）
nodes:
  - role: control-plane                    # 1 个控制平面节点
  - role: worker                           # 2 个工作节点
  - role: worker
```

### 2.2 创建集群

```powershell
kind create cluster --config kind-config.yaml
```

> 因为配置里写了 `name: rbs`，集群自动命名为 `rbs`。首次要拉取 kind 节点镜像，**需要几分钟**。

**期望输出**（最后几行）：

```
Creating cluster "rbs" ...
 ✓ Ensuring node image (kindest/node:v1.32.x) 🖼
 ✓ Preparing nodes 📦
 ✓ Writing configuration 📜
 ...
 ✓ Total time to wait for the control plane: 30s
Your cluster has been created in 1m12s
```

### 2.3 验证集群

```powershell
kubectl cluster-info --context kind-rbs
kubectl get nodes -o wide
```

**期望输出**（3 个节点，全部 `Ready`）：

```
NAME                 STATUS   ROLES           AGE   VERSION
rbs-control-plane    Ready    control-plane   1m    v1.32.x
rbs-worker           Ready    <none>          1m    v1.32.x
rbs-worker2          Ready    <none>          1m    v1.32.x
```

---

## 第 3 章 认识你的集群

### 3.1 节点 = 容器

kind 的"节点"其实都是 Docker 容器。用 Docker 视角看看：

```powershell
docker ps --filter name=rbs
```

能看到 `rbs-control-plane`、`rbs-worker`、`rbs-worker2` 三个容器——这就是"K8s 节点"。

### 3.2 为什么 Pod 不跑到 control-plane 上

```powershell
kubectl get node rbs-control-plane -o jsonpath='{.spec.taints}'
```

会看到 `NoSchedule` 污点——K8s 用它把普通 Pod 挡在控制平面之外（控制平面专心管集群）。这就是"多节点部署"的意义：**Pod 分散到多个 worker 上**。

---

## 第 4 章 构建镜像并导入集群

### 4.1 为什么必须 `kind load`

kind 的节点是容器，**默认访问不到你本机 Docker daemon 里的镜像**（节点里跑的是 containerd，有自己的镜像存储）。所以要把构建好的镜像**导入**到每个节点。

### 4.2 构建镜像

在你仓库的 `web` 目录构建（和之前一样，会从 GitHub 拉代码）：

```powershell
cd E:\Demo\source\repos\RentalBillingSystem\web
docker compose build
```

**或者**用本地目录构建（不走远程）也行：

```powershell
docker build -t web-web .
```

两种方式产物都是 `web-web:latest`。验证：

```powershell
docker images web-web
```

### 4.3 导入集群

```powershell
kind load docker-image web-web:latest --name rbs
```

**期望输出**：

```
Image: "web-web:latest" with ID "sha256:xxx" not yet present on node "rbs-worker", loading...
Image: "web-web:latest" with ID "sha256:xxx" not yet present on node "rbs-worker2", loading...
Image: "web-web:latest" with ID "sha256:xxx" not yet present on node "rbs-control-plane", loading...
```

> 每次重新构建镜像后，都要重新 `kind load` 一次（K8s 里"节点本机没有的镜像"才需要，生产环境则用镜像仓库 `imagePullPolicy: IfNotPresent` 或直接从仓库拉）。

---

## 第 5 章 编写部署清单（Deployment + Service）

在 `web` 目录（或任意目录）创建 **两个 YAML 文件**。

### 5.1 Deployment（`web-deployment.yaml`）

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rbs-web
  labels:
    app: rbs-web
spec:
  replicas: 3                       # 3 个副本，会分布到 2 个 worker 节点
  selector:
    matchLabels:
      app: rbs-web                  # 必须与 template 的 label 一致（管理 Pod 的规则）
  template:
    metadata:
      labels:
        app: rbs-web
    spec:
      # 让 Pod 能解析 www.rbsapi.com → 宿主机（IIS 后端）。
      # IP 先填占位，第 7 章教你查出真实值。相当于 Docker 的 extra_hosts。
      hostAliases:
        - ip: "请替换为宿主机IP"
          hostnames:
            - "www.rbsapi.com"
      containers:
        - name: rbs-web
          image: web-web:latest
          imagePullPolicy: IfNotPresent   # 节点本机有镜像就用本机的（因为我们是 kind load 导入的）
          ports:
            - containerPort: 80           # nginx 监听端口
          resources:
            requests:
              cpu: 100m
              memory: 128Mi
            limits:
              cpu: 500m
              memory: 256Mi
```

### 5.2 Service（`web-service.yaml`）

```yaml
apiVersion: v1
kind: Service
metadata:
  name: rbs-web-svc
spec:
  type: NodePort                       # 暴露到每个节点的一个固定端口
  selector:
    app: rbs-web                       # 选择所有 label 为 app=rbs-web 的 Pod
  ports:
    - port: 80                         # 集群内访问端口
      targetPort: 80                   # Pod 里的 nginx 端口
      nodePort: 30882                  # 节点上暴露的端口（30000-32767）
```

### 5.3 和 docker-compose 对照（关键理解）

| compose 写法 | K8s 写法 |
|---|---|
| `services.web`（一个容器） | Deployment（replicas 个 Pod，每个 Pod 一个容器） |
| `container_name: rbs-web` | `metadata.name`（但不是唯一的，有副本） |
| `ports: "8082:80"` | Service NodePort `nodePort: 30882, targetPort: 80` |
| `extra_hosts: www.rbsapi.com:host-gateway` | `hostAliases`（Pod 的 /etc/hosts） |
| `restart: unless-stopped` | Deployment 自动保证副本数（自愈） |

---

## 第 6 章 部署并访问前端

### 6.1 应用清单

```powershell
kubectl apply -f web-deployment.yaml -f web-service.yaml
```

**期望输出**：

```
deployment.apps/rbs-web created
service/rbs-web-svc created
```

### 6.2 看 Pod 分布（重点：多节点）

```powershell
kubectl get pods -o wide
```

**期望输出**（3 个 Pod 分布在 2 个 worker 节点上）：

```
NAME                       READY   STATUS    RESTARTS   AGE   IP           NODE
rbs-web-6f7d4b8f9c-abc12   1/1     Running   0          30s   10.244.1.4   rbs-worker
rbs-web-6f7d4b8f9c-def34   1/1     Running   0          30s   10.244.1.5   rbs-worker2
rbs-web-6f7d4b8f9c-ghi56   1/1     Running   0          30s   10.244.2.3   rbs-worker2
```

> 注意 NODE 列：Pod 被调度到了不同的 worker 节点——这就是"多节点"。

### 6.3 看 Service

```powershell
kubectl get svc rbs-web-svc
```

**期望输出**：

```
NAME          TYPE       CLUSTER-IP      EXTERNAL-IP   PORT(S)        AGE
rbs-web-svc   NodePort   10.96.xx.xx     <none>        80:30882/TCP   30s
```

### 6.4 浏览器访问

kind 在 Docker Desktop 上把 NodePort 自动映射到了宿主机，所以直接访问：

```
http://localhost:30882
```

**期望**：看到登录页。说明 K8s 集群里的 nginx 已经把前端静态文件吐出来了 🎉。

---

## 第 7 章 打通后端 /api（hostAliases 详解）

> 前端页面能开，但**登录会失败**，因为 Pod 里 `www.rbsapi.com` 还没解析到宿主机（后端在 Windows 的 IIS 上）。这一步解决它。

### 7.1 原理

- Docker 的 `extra_hosts: host-gateway` 只在 compose 里生效；K8s 的 Pod 不认识它。
- K8s 给 Pod 写 `/etc/hosts` 的正规方式是 **`hostAliases`**（在 Deployment 里声明）。
- 但"宿主机 IP"要从 kind 节点的视角看——因为 Pod 的路由要经过节点才能到宿主机。

### 7.2 查出宿主机 IP（从节点视角）

```powershell
docker exec rbs-worker getent hosts host.docker.internal
```

**期望输出**（第一列就是宿主机 IP，形如 `192.168.65.x`）：

```
192.168.65.1    host.docker.internal
```

> Docker Desktop 里，`host.docker.internal` 专门用来指代"宿主机"。kind 节点是容器，能解析到它。这个 IP 就是 Pod 访问宿主机要走的路由目标。
>
> 如果上面命令没输出，用备选：查 Windows 本机局域网 IP，`ipconfig` 里找 IPv4 地址（如 `192.168.31.19`），填入即可。

### 7.3 把真实 IP 填进 Deployment

把 `web-deployment.yaml` 里的占位替换成刚查到的 IP：

```yaml
      hostAliases:
        - ip: "192.168.65.1"      # ← 换成 7.2 查到的真实 IP
          hostnames:
            - "www.rbsapi.com"
```

### 7.4 重新应用（滚动更新）

```powershell
kubectl apply -f web-deployment.yaml
kubectl rollout status deployment/rbs-web
```

### 7.5 验证后端联通

```powershell
# 找任意一个 Pod 名
kubectl get pods -l app=rbs-web
# 进 Pod 测试后端连通性
kubectl exec -it <Pod名> -- wget -qO- http://www.rbsapi.com/api/health
```

**期望**：返回后端健康信息。然后回到浏览器登录（admin / admin123），**登录成功 = 全链路通**：

```
浏览器 localhost:30882 → NodePort → Service → Pod nginx → www.rbsapi.com(hostAliases→宿主机) → IIS → SQL Server
```

---

## 第 8 章 生产操作：扩缩容 / 滚动更新 / 自愈

### 8.1 扩缩容

```powershell
kubectl scale deployment rbs-web --replicas=5
kubectl get pods -o wide     # 变 5 个，继续分散到节点
```

缩回：`kubectl scale deployment rbs-web --replicas=2`

### 8.2 滚动更新（改代码后发布新版本）

```powershell
# 1. 改前端代码 → 重新构建 → 重新导入（别漏这步！）
cd E:\Demo\source\repos\RentalBillingSystem\web
docker compose build
kind load docker-image web-web:latest --name rbs

# 2. 触发滚动更新（K8s 会逐个替换 Pod，不中断服务）
kubectl rollout restart deployment rbs-web
kubectl rollout status deployment rbs-web
```

**观察滚动过程**：

```powershell
kubectl get pods -w
```

会看到旧 Pod 逐个 `Terminating`、新 Pod 逐个 `Running`，**全程可访问**——这就是滚动更新。

### 8.3 自愈（K8s 的核心卖点）

```powershell
# 故意删一个 Pod
kubectl delete pod rbs-web-6f7d4b8f9c-abc12
kubectl get pods          # 立刻自动新建一个顶上来
```

Deployment 保证"永远有 3 个 Running 的 Pod"——容器死了、Pod 挂了、节点挂了，都会自动补。这就是 `restart: unless-stopped` 的 K8s 升级版。

### 8.4 看详细状态

```powershell
kubectl describe deployment rbs-web    # 部署详情
kubectl logs <Pod名>                    # 看某个 Pod 日志
kubectl get events --sort-by=.lastTimestamp   # 集群事件
```

---

## 第 9 章 常用命令速查

```powershell
# 集群
kind create cluster --config kind-config.yaml   # 建集群
kind get clusters                                # 看集群
kind delete cluster --name rbs                   # 删集群
kind load docker-image web-web:latest --name rbs # 导镜像

# 部署对象
kubectl apply -f xx.yaml        # 应用/更新清单
kubectl get nodes / pods / svc / deployment -o wide
kubectl scale deployment rbs-web --replicas=5
kubectl rollout restart deployment rbs-web
kubectl rollout status deployment rbs-web
kubectl describe pod <名> / deployment rbs-web
kubectl logs <Pod名>
kubectl exec -it <Pod名> -- sh
kubectl delete -f xx.yaml       # 删除清单
```

---

## 第 10 章 排错表

| 现象 | 原因 | 解决 |
|---|---|---|
| Pod `ImagePullBackOff` / `ErrImagePull` | 节点上没有该镜像 | 重新 `kind load docker-image web-web:latest --name rbs` |
| Pod `Pending` 卡住 | 没有 worker 可调度 / 资源不够 | `kubectl get nodes` 看 Ready；`describe pod` 看事件 |
| Pod `CrashLoopBackOff` | 容器启动即退出 | `kubectl logs <Pod>`；多半 nginx 配置/端口问题 |
| 页面 502 / 登录网络失败 | Pod 里解析不到宿主机后端 | 第 7 章：确认 hostAliases IP 正确；`kubectl exec` 里 `wget` 后端 |
| `localhost:30882` 打不开 | Service 没建好 / 端口被占 | `kubectl get svc` 确认 nodePort；换 30000-32767 里其它端口 |
| `kind create` 拉节点镜像慢 | 网络问题 | 配了 Docker 镜像加速后通常 OK；多等一会 |
| kubectl 连接失败 | 上下文不对 | `kubectl cluster-info --context kind-rbs`；`kubectl config get-contexts` |
| Pod 都在一个节点上 | 副本少、调度正常 | 正常现象；`replicas` 大于 worker 数且节点有空才会分散 |

---

## 第 11 章 清理

```powershell
# 删除部署（可选，只删对象不删集群）
kubectl delete deployment rbs-web
kubectl delete service rbs-web-svc

# 彻底删掉整个集群（干净利落，不留任何东西）
kind delete cluster --name rbs

# 验证
kind get clusters          # 空的
kubectl config get-contexts  # kind-rbs 消失
```

---

## 附录 A K8s 面试要点（速记版）

1. **Pod vs 容器**：Pod 是最小调度单位，可包含多个紧密协作的容器（共享网络/存储）；容器是 Pod 内的运行单元。
2. **Deployment 做什么**：声明副本数、镜像、更新策略；负责创建 Pod、滚动更新、自愈（保副本数）、回滚。
3. **Service 做什么**：给一组易变的 Pod 提供稳定的访问入口，内置负载均衡（ClusterIP 集群内 / NodePort 暴露节点端口 / LoadBalancer 对外）。
4. **NodePort 端口范围**：30000–32767。
5. **滚动更新 vs 重建**：滚动更新逐个替换、不中断；`kubectl set image` / `rollout restart` 触发。
6. **自愈**：Deployment/ReplicaSet 保证 Pod 数；kubelet 监控容器健康；探针（liveness/readiness）控制恢复。
7. **Taint/Toleration、Label/Selector**：Taint 决定 Pod 不能调度到哪些节点；Label+Selector 决定 Service 管哪些 Pod。
8. **hostAliases**：给 Pod /etc/hosts 写自定义域名映射（K8s 里的 extra_hosts）。
9. **kind 原理**：用 Docker 容器模拟 K8s 节点，适合本地学习/CI；生产用云托管 K8s（AKS/EKS/GKE）或自建 K3s/Kubeadm。
10. **和 Docker Compose 的关系**：Compose 管单机多容器；K8s 管跨多机多容器，且多出调度、自愈、滚动更新、服务发现等能力。

---

*配套文档：《[前端Docker部署指南](前端Docker部署指南.md)》（单机 Docker）、《[Docker远程仓库部署指南](Docker远程仓库部署指南.md)》（远程仓库）、《[Docker部署学习教程](Docker部署学习教程.md)》（Docker 全方位含面试题）。*
