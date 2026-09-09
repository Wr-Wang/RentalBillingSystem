# 生产高可用部署计划：3 Master + 2 Worker（RBS API×3 + Web×3 全容器化）

> 适用：把 RentalBillingSystem 部署到**标准高可用生产 Kubernetes 集群**。
> 架构：**5 台服务器，3 Master（控制面）+ 2 Worker（业务）**，kubeadm 搭建，Ubuntu 22.04/24.04。
> 业务规模：**RBS.Api 3 副本 + web 3 副本**，双域名 ingress 路由，SQL Server 独立于集群之外。
>
> 本计划基于 `docs/K8sKind多节点部署教程-API+Web全容器化.md`（本机 kind 单集群教程）改写。核心差异：
>
> | 维度 | kind 教程（本机） | 本计划（生产） |
> |---|---|---|
> | 集群形态 | kind 容器模拟（1 控制面 + 6 worker） | kubeadm 真实服务器（3 控制面 + 2 worker） |
> | 控制面高可用 | 无（单控制面） | etcd 3 节点 + keepalived/nginx VIP 负载均衡 |
> | 镜像来源 | 本机构建 + `kind load` | **自建 Harbor 私有仓库**，节点从 Harbor 拉取 |
> | 域名解析 | hosts 文件 | 内网 DNS A 记录 → worker-1/2（生产不要用 hosts） |
> | API 副本 | 3 | 3（并发安全已由原子条件插入保证，直接可用） |
>
> 前置建议先读 kind 教程的 **附录 A**：PDF 跨平台修复（`NotePdf.cs` 已移除 System.Drawing）。用当前仓库代码构建的镜像可直接在 Linux 上生成 PDF，无需再改。

---

## 0. 架构总览

### 0.1 服务器与 IP 规划（示例内网网段 `192.168.10.0/24`）

| 角色 | 主机名 | IP | 部署内容 |
|---|---|---|---|
| 负载均衡 VIP | — | **192.168.10.100** | keepalived 在 3 台 Master 间漂移；nginx 在此监听 6443 转发给 3 台 Master |
| Master 1 | master-1 | 192.168.10.11 | etcd + kube-apiserver/scheduler/controller-manager + nginx + keepalived |
| Master 2 | master-2 | 192.168.10.12 | 同上 |
| Master 3 | master-3 | 192.168.10.13 | 同上 |
| Worker 1 | worker-1 | 192.168.10.21 | kubelet + kube-proxy + 业务 Pod（api/web）+ **Harbor 私有仓库**（妥协方案，见 0.5） |
| Worker 2 | worker-2 | 192.168.10.22 | kubelet + kube-proxy + 业务 Pod（api/web） |
| 数据库 | rbs-db | 192.168.10.30 | **集群外**：SQL Server 2019/2022，TCP 1433（可以是本机 Windows，或独立 DB 服务器） |

> ⚠️ IP/网段、主机名、**网卡名**（ens33/ens160/eth0…）请按实际环境替换，下文统一用示例值。
> ⚠️ 所有 5 台服务器都要能互相 ping 通、能访问 Harbor（192.168.10.21:8081）、能访问 SQL（192.168.10.30:1433）。

### 0.2 网络拓扑

```
                          ┌───────────────────────── 内网 192.168.10.0/24 ─────────────────────────┐
   客户端                   │                                                                            │
 (浏览器/运维)              │  VIP 192.168.10.100（keepalived 在 3 台 Master 间漂移）                     │
     │                      │        │                                                                  │
     │                      │   nginx stream :6443（四层转发，只绑 VIP）                                   │
     │                      │        └───────────► 3 × kube-apiserver:6443                             │
     │                      │                                                                            │
     ▼                      │  www.rbsapi.com ──┐                                                       │
   DNS ──► worker-1/2 IP ◄──┴──► ingress-nginx hostNetwork :80/443（每 worker 1 个控制器）                │
   (A 记录)                     │        │                                                              │
      www.rbsapi.com           │   rbs-api-ingress ──► rbs-api-svc:8080 ──► api Pod × 3                 │
      www.rbsadmin.com         │   rbs-web-ingress ──► rbs-web-svc:80 ──► web Pod × 3 ─(/api 反代)──► rbs-api-svc
     │                         │                                                                            │
     ▼                         └── api Pod ──(连接串直连)──► SQL Server 192.168.10.30:1433                  │
                               └────────────────────────────────────────────────────────────────────────┘
```

请求路径：`浏览器 → DNS → worker 节点 IP:80/443 → ingress-nginx → Service → Pod`；
API Pod 访问数据库走**节点网关 NAT**直接到 `192.168.10.30:1433`（生产 Pod 默认可出网，不需要 kind 那种 hostAliases）。

### 0.3 组件版本（与现有 kind 集群保持一致，便于复用镜像）

| 组件 | 版本 |
|---|---|
| 操作系统 | Ubuntu 22.04 LTS / 24.04 LTS |
| Kubernetes | **v1.36.3**（kubeadm / kubelet / kubectl） |
| 容器运行时 | **containerd.io 1.7+**（所有 5 台；Harbor 机额外装 docker-ce 守护进程） |
| CNI | **Calico v3.29.x**（Pod 网段 10.244.0.0/16） |
| Ingress | **ingress-nginx v1.12.1**（Deployment replicas=2 + hostNetwork，每 worker 1 个控制器） |
| 私有仓库 | **Harbor v2.11+**（装在 worker-1，HTTP 端口 **8081**，避开 ingress-nginx 的 80） |
| API Server LB | keepalived 2.x + nginx（stream 模块） |
| SQL Server | 2019 / 2022（集群外，独立） |

### 0.4 端口清单

| 方向 | 端口 | 用途 | 涉及主机 |
|---|---|---|---|
| 集群内 | 6443 | kube-apiserver | 3 台 Master |
| 集群内 | 2379 / 2380 | etcd client / peer | 3 台 Master（互相） |
| 集群内 | 10250 | kubelet | 全部 5 台（互相信任） |
| 集群内 | 10259 / 10257 / 10252 | kube-scheduler / kube-controller-manager | 3 台 Master |
| 集群内 | 6783 / 9098 | Calico VXLAN/BGP | 全部 5 台 |
| 集群内 | 8080 / 80 | api-service / web-service（ClusterIP，仅集群内） | 不对外开放 |
| 对外 | 80 / 443 | ingress-nginx（hostNetwork） | worker-1、worker-2 |
| 对外（兜底） | 30883 | web NodePort | worker-1、worker-2 |
| 对外 | 8081（Harbor） | Harbor HTTP（用 8081，避免与 ingress-nginx 的 80 冲突） | worker-1 |
| 对外（数据库侧） | 1433 | SQL Server TCP | rbs-db（192.168.10.30） |

> 生产建议用 `ufw` 或云安全组按上表放行；**不推荐直接关闭防火墙**。本计划在 1.7 提供 ufw 命令示例。

### 0.5 关键设计决策

1. **API replicas=3 并发安全**：`SqlMaps.xml` 的 `Scheduling.Insert.Execution.Default` 已改为原子条件插入（`UPDLOCK+HOLDLOCK` 的 `WHERE NOT EXISTS`），多副本并发生成只落 1 条。生产直接 `replicas: 3`，无需回退单副本。
2. **Harbor 装在 worker-1**：5 台服务器没有独立基础设施机，故让 worker-1 兼任 Harbor 宿主（Docker 守护进程与 kubelet 并存，官方允许）。Harbor 用 **HTTP 端口 8081**——因为 ingress-nginx 的 hostNetwork 会占用 worker 的 80/443（见 5.4），两者不能抢 80。**若条件允许，强烈建议用第 6 台独立虚机装 Harbor**，worker-1 只跑业务。
3. **API Server 高可用**：nginx（stream 四层）跑在 3 台 Master 上，只监听 **VIP:6443**，转发到 3 台 Master 的 6443；keepalived 负责 VIP 漂移。所有 kubeadm init/join 都用 VIP 作 `controlPlaneEndpoint`。
4. **web/API 入口**：ingress-nginx Deployment `replicas: 2` + `hostNetwork: true`，恰好每 worker 1 个控制器，监听节点 IP 的 80/443。DNS 两个域名都 A 记录到 worker-1、worker-2。NodePort 30883 保留作兜底。
5. **生产域名不用 hosts**：内网 DNS 配 A 记录即可；客户端（如本机）临时测试想直接用域名，可先加 hosts，正式使用请改 DNS。
6. **SQL 连接串**：`Server=192.168.10.30,1433`（生产 Pod 直连数据库 IP，无需 hostAliases）。若 SQL 与本计划同一内网但禁止 Pod 出网，则需在 CNI/网络策略层放行 1433。

### 0.6 涉及文件（相对仓库根）

| 文件 | 说明 |
|---|---|
| `k8s/production/api-deployment.yaml` | API Deployment：`replicas: 3`，镜像指向 Harbor，连接串指向数据库 IP，探针 `/swagger/v1/swagger.json` |
| `k8s/production/api-service.yaml` | ClusterIP `rbs-api-svc:8080` |
| `k8s/production/web-deployment.yaml` | web Deployment：`replicas: 3`，镜像指向 Harbor |
| `k8s/production/web-service-nodeport.yaml` | NodePort：集群内 80 + 兜底 30883 |
| `k8s/production/ingress.yaml` | 双域名 Ingress（`www.rbsapi.com`→api、`www.rbsadmin.com`→web） |
| `k8s/production/kubeadm-config.yaml` | kubeadm init 配置（endpoint=VIP、podSubnet=10.244.0.0/16、镜像仓库国内源） |
| `k8s/production/calico.yaml` | Calico 清单（IP 池已改为 10.244.0.0/16，镜像可指向 Harbor） |
| `k8s/production/ingress-nginx.yaml` | ingress-nginx 清单（改 hostNetwork + replicas=2，镜像可指向 Harbor） |
| `src/RBS.Api/Dockerfile` / `web/Dockerfile` | 与 kind 教程相同，直接复用 |

> 本计划不新建这些文件，只给出内容与修改说明；你也可以把 kind 教程里已有的 `k8s/api-deployment.yaml` 等按 5、6 章的说明改造成生产版。

---

## 1. 服务器通用准备（5 台都要做）

> 以下所有命令用 root 或 sudo 执行。每台服务器做完 `1.7 验证` 通过后再进入下一步。

### 1.1 硬件要求（单台）

| 角色 | CPU | 内存 | 磁盘 |
|---|---|---|---|
| Master | 2 核+（建议 4 核） | 4 GB+（建议 8 GB） | 40 GB+（etcd + 系统） |
| Worker | 2 核+（建议 4 核） | 4 GB+（建议 8 GB） | 80 GB+（镜像 + 容器层） |
| 数据库机 | — | 8 GB+ | 视数据量 |

> 本次 6 个业务 Pod（api×3 + web×3）+ Harbor + ingress 控制器，2 台 worker 每台约 3 个业务 Pod + 1 个 ingress 控制器，Worker 建议 4 核 / 8 GB 起步。

### 1.2 设置主机名与 hosts（5 台各自执行）

```bash
# 每台服务器改成自己的主机名（master-1 / master-2 / master-3 / worker-1 / worker-2）
sudo hostnamectl set-hostname master-1
```

```bash
# 5 台服务器都追加同一份 hosts（把下面的 IP/主机名替换成你的实际值；VIP 也写上方便记忆）
sudo tee -a /etc/hosts <<'EOF'
192.168.10.11 master-1
192.168.10.12 master-2
192.168.10.13 master-3
192.168.10.21 worker-1
192.168.10.22 worker-2
192.168.10.100 k8s-vip
EOF
```

### 1.3 关闭 swap

```bash
# ① 临时关闭本会话的 swap
sudo swapoff -a
# ② 永久关闭：用 sed 把 /etc/fstab 里含 swap 的行注释掉（原文件备份为 /etc/fstab.bak）
sudo sed -i.bak '/swap/s/^/#/' /etc/fstab
# 验证：下面命令应无输出（无 swap 分区列表 = 已关闭）
sudo swapon --show
```

> 原因：kubelet 在 swap 开启时无法工作（Kubelet 默认 `failSwapOn=true`），k8s 官方要求关闭 swap。

### 1.4 加载内核模块与系统参数

```bash
# ① 需要的内核模块：overlay（容器镜像层）、br_netfilter（iptables 处理桥接流量）
cat <<EOF | sudo tee /etc/modules-load.d/k8s.conf
overlay
br_netfilter
EOF
sudo modprobe overlay && sudo modprobe br_netfilter
# 验证（两条都应打印模块名）
lsmod | grep -E 'overlay|br_netfilter'

# ② 内核参数：让 iptables 能处理 bridge 流量、开启 IPv4 转发（Pod 间及出网需要）
cat <<EOF | sudo tee /etc/sysctl.d/k8s.conf
net.bridge.bridge-nf-call-iptables  = 1
net.bridge.bridge-nf-call-ip6tables = 1
net.ipv4.ip_forward                 = 1
EOF
sudo sysctl --system
# 验证：下面应输出 1
sysctl net.bridge.bridge-nf-call-iptables net.ipv4.ip_forward
```

### 1.5 安装容器运行时 containerd.io（5 台相同）

> 统一装 docker-ce 源里的 `containerd.io`（比 Ubuntu 自带 containerd 新且稳定），并从同一源给 worker-1 后续装 docker 守护进程，避免包冲突。

```bash
# ① 装依赖与 docker-ce 仓库（国内用阿里云镜像源）
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg
curl -fsSL https://mirrors.aliyun.com/docker-ce/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://mirrors.aliyun.com/docker-ce/linux/ubuntu $(. /etc/os-release && echo $VERSION_CODENAME) stable" | sudo tee /etc/apt/sources.list.d/docker.list
sudo apt-get update

# ② 只装 containerd（worker-1 后续装 Harbor 时再补装 docker-ce，见第 3 章）
sudo apt-get install -y containerd.io
```

```bash
# ③ 生成默认配置并改为 systemd cgroup（kubelet 与 containerd 必须一致）
sudo mkdir -p /etc/containerd
containerd config default | sudo tee /etc/containerd/config.toml >/dev/null
sudo sed -i 's/SystemdCgroup = false/SystemdCgroup = true/' /etc/containerd/config.toml

# ④ 把 pause 沙箱镜像换成国内可达的（默认 registry.k8s.io/pause:3.x 国内拉不动）
sudo sed -i 's#sandbox_image = "registry.k8s.io/pause:[^"]*"#sandbox_image = "registry.aliyuncs.com/google_containers/pause:3.10"#' /etc/containerd/config.toml

# ⑤ 重启并开机自启
sudo systemctl enable --now containerd
sudo systemctl restart containerd

# 验证
containerd --version
```

### 1.6 安装 kubeadm / kubelet / kubectl（5 台相同，版本固定）

```bash
# ① 加 k8s 官方 apt 源（国内网络走阿里云镜像，见后备）
sudo apt-get install -y apt-transport-https ca-certificates curl gpg
curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.36/deb/Release.key | sudo gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg
echo 'deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.36/deb/ /' | sudo tee /etc/apt/sources.list.d/kubernetes.list
sudo apt-get update

# ② 装固定版本并 hold 住（防止误升级把集群搞挂）
sudo apt-get install -y kubelet=1.36.3-1.1 kubeadm=1.36.3-1.1 kubectl=1.36.3-1.1
sudo apt-mark hold kubelet kubeadm kubectl
```

> **国内网络后备**：若 `pkgs.k8s.io` 拉不动，改用阿里云 k8s 源：
> ```bash
> curl -fsSL https://mirrors.aliyun.com/kubernetes/apt/doc/apt-key.gpg | sudo gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg
> echo 'deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://mirrors.aliyun.com/kubernetes/apt/ kubernetes-xenial main' | sudo tee /etc/apt/sources.list.d/kubernetes.list
> sudo apt-get update
> # 阿里云源的版本号后缀是 -00（不是 -1.1）：
> sudo apt-get install -y kubelet=1.36.3-00 kubeadm=1.36.3-00 kubectl=1.36.3-00
> sudo apt-mark hold kubelet kubeadm kubectl
> ```

### 1.7 通用验证（5 台都执行）

```bash
kubectl version --client            # 输出 1.36.3（Client）
kubeadm version                     # 输出 1.36.3
containerd --version                # 1.7.x
sudo systemctl is-active containerd # active
```

---

## 2. API Server 高可用：nginx + keepalived（3 台 Master）

> 目标：`kubeadm` 用它拿到的**一个固定入口** = `192.168.10.100:6443`（VIP）。任何一台 Master 挂了，keepalived 把 VIP 漂到存活 Master，nginx 只把流量转发到存活的 6443。kubelet、kubectl、kubeadm 全都连这个 VIP，控制面对外表现为"一个"。

### 2.1 安装 nginx 并配置 6443 四层转发（3 台 Master 相同）

```bash
# ① 装 nginx（Ubuntu 官方源已编译 stream 模块，无需额外编译）
sudo apt-get install -y nginx
# ② 验证 stream 模块已加载（应输出 ngx_stream_module.so 路径）
grep -r stream /etc/nginx/modules-enabled/ 2>/dev/null
```

```bash
# ③ 写四层转发配置：只监听 VIP:6443，转发到 3 台 Master 的 6443
sudo tee /etc/nginx/conf.d/k8s-apiserver-lb.conf <<'EOF'
stream {
    upstream kube_apiserver {
        server 192.168.10.11:6443 max_fails=3 fail_timeout=30s;
        server 192.168.10.12:6443 max_fails=3 fail_timeout=30s;
        server 192.168.10.13:6443 max_fails=3 fail_timeout=30s;
    }
    server {
        listen 192.168.10.100:6443;      # 只绑 VIP（不绑 0.0.0.0，避免与本机 apiserver 6443 冲突）
        proxy_pass kube_apiserver;
        proxy_connect_timeout 5s;
        proxy_timeout 30s;
    }
}
EOF
sudo nginx -t
sudo systemctl restart nginx
```

```bash
# ④ 关键：允许 nginx 绑定"本机还没有的 VIP 地址"（ip_nonlocal_bind）。
#    不设这条，nginx 启动会报 "cannot assign requested address"（因为 VIP 平时只存在一台机器上）。
echo 'net.ipv4.ip_nonlocal_bind = 1' | sudo tee -a /etc/sysctl.d/99-k8s-lb.conf
sudo sysctl -p /etc/sysctl.d/99-k8s-lb.conf
sudo systemctl restart nginx
```

> ⚠️ 不要用 `listen 0.0.0.0:6443`——本机 kube-apiserver 也监听 6443，两个绑定会冲突。必须只绑 VIP。
> ⚠️ 配置里的 3 个 `server` 用**内网真实 IP**（不是 VIP），保证转发打到具体 Master。

### 2.2 安装 keepalived 并配置 VIP 漂移（3 台 Master）

```bash
# ① 装 keepalived
sudo apt-get install -y keepalived
```

```bash
# ② 查自己的网卡名（下面配置的 interface 要用）
ip -4 addr show | grep -E '^[0-9]+:|inet ' | head -20
# 例如：ens33: <BROADCAST...>   ← 这就是网卡名；或 ens160 / eth0 / enp1s0
```

master-1 的配置（**priority 100，state MASTER**）：

```bash
sudo tee /etc/keepalived/keepalived.conf <<'EOF'
global_defs {
    router_id k8s_master_1
    vrrp_skip_check_adv_addr
}
vrrp_instance k8s_apiserver {
    state MASTER
    interface ens33                       # ← 改成实际网卡名
    virtual_router_id 51
    priority 100                          # master-2=90, master-3=80
    advert_int 1
    authentication {
        auth_type PASS
        auth_pass K8s2026                 # ≤8 位，3 台必须一致
    }
    virtual_ipaddress {
        192.168.10.100/24 dev ens33       # ← 网卡名同样改
    }
}
EOF
sudo systemctl enable --now keepalived
```

master-2 的配置（**priority 90，state BACKUP**）、master-3（**priority 80，state BACKUP**）与上面完全一样，只改：`router_id`、`state=BACKUP`、`priority`。

### 2.3 验证 VIP 与 6443 转发（master-1 上执行）

```bash
# ① VIP 应出现在 master-1 上（ip addr 里能看到 192.168.10.100）
ip addr show | grep '192.168.10.100'

# ② 通过 VIP 访问 apiserver 应能返回 401/200（此时 apiserver 还没 init，会报连接拒绝属正常；
#    初始化控制面后（第 4 章）再执行一次，应看到 401 Unauthorized = 端口已通）
curl -k https://192.168.10.100:6443/version
```

> 若 VIP 没在 master-1 上：`sudo systemctl status keepalived`、`sudo journalctl -u keepalived -n 50` 看日志。
> 常见原因：网卡名写错、`auth_pass` 不一致、`virtual_router_id` 不一致。

---

## 3. Harbor 私有仓库（worker-1）

> 5 台服务器方案下 Harbor 装在 worker-1。它会与本机 kubelet 并存，官方允许；若你有第 6 台机器，把本节整体搬到那台，其余章节的 `192.168.10.21:8081` 改成 Harbor 新地址+端口即可。

### 3.1 安装 Docker Engine（worker-1）

```bash
# docker-ce 源已在 1.5 加好，直接补装守护进程
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli docker-compose-plugin
sudo systemctl enable --now docker

# 让当前用户能免 sudo 用 docker（可选；重新登录生效）
sudo usermod -aG docker $USER

docker version          # Client + Server 都要有版本号
```

> ⚠️ 关键：worker-1 的 kubelet 用的是 **containerd**（1.5 装的），docker 守护进程也装在这里。
> 两者共用同一个 `containerd.io` 二进制包，但 Docker 管理自己的容器，**不冲突**。
> 不要在生产节点上跑 `docker` 容器业务，Harbor 例外。

### 3.2 安装 Harbor

```bash
# ① 下载离线安装包（GitHub 慢的话用 gitee 镜像或科学上网；国内可搜 harbor-offline-installer 镜像站）
wget https://github.com/goharbor/harbor/releases/download/v2.11.1/harbor-offline-installer-v2.11.1.tgz
tar xzvf harbor-offline-installer-v2.11.1.tgz
cd harbor

# ② 生成配置
cp harbor.yml.tmpl harbor.yml
sudo vim harbor.yml
```

编辑 `harbor.yml` 要点：

```yaml
hostname: 192.168.10.21          # 用 IP 作主机名（内网免 DNS）
# 注释掉 https 整段（内网用 http 简化；生产外网建议配 https 证书）
# https:
#   port: 443
#   ...
http:
  port: 8081              # 用非 80 端口：ingress-nginx 的 hostNetwork 要占 worker 的 80
# harbor_admin_password: Harbor12345   # 初始管理员密码（可改）
```

```bash
# ③ 安装（自动拉取 harbor 各组件镜像并 docker-compose 起服务）
sudo ./install.sh
# ④ 验证：能看到 harbor-core / registry / portal / postgresql / redis 等容器 Running
docker ps | grep goharbor
```

> 浏览器打开 `http://192.168.10.21:8081` 用 `admin / <harbor_admin_password>` 登录。先在 UI 里建一个项目 **rbs**（公开/私有均可，后文按私有写）。

### 3.3 各节点 containerd 信任 Harbor（5 台都执行）

Harbor 是 **http 非加密**仓库，containerd 默认拒绝非 https。用 containerd 的 `certs.d` 目录声明"信任并走 http"：

```bash
# ① 在 /etc/containerd/config.toml 里把 registry 改为目录模式
sudo tee -a /etc/containerd/config.toml <<'EOF'

[plugins."io.containerd.grpc.v1.cri".registry]
  config_path = "/etc/containerd/certs.d"
EOF

# ② 为 Harbor 地址建信任配置（目录名必须带上端口 8081，containerd 按"主机:端口"找配置）
sudo mkdir -p /etc/containerd/certs.d/192.168.10.21:8081
sudo tee /etc/containerd/certs.d/192.168.10.21:8081/hosts.toml <<'EOF'
server = "http://192.168.10.21:8081"

[host."http://192.168.10.21:8081"]
  capabilities = ["pull", "resolve"]
EOF

# ③ 重启 containerd 生效
sudo systemctl restart containerd
```

> 若以后加别的镜像仓库，照同样结构建 `/etc/containerd/certs.d/<地址>/hosts.toml` 即可。

### 3.4 构建并推送 API / Web 镜像

> 构建机可以是 worker-1（有 docker），也可以是你的开发机（Windows/Docker Desktop）。下面以 worker-1 为例；**在开发机构建时**，记得先 `docker login 192.168.10.21:8081`，并给 Docker 加 insecure-registry（Windows Docker Desktop → Settings → Docker Engine 加 `"insecure-registries":["192.168.10.21:8081"]`）。

```bash
# ① 从仓库把代码拷到 worker-1（或用 git clone），进入仓库根目录
# ② 拉基础镜像（国内镜像源拉完改名，见 kind 教程 3.1/3.2）
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/aspnet:10.0
docker pull docker.1panel.live/library/node:22-alpine && docker tag docker.1panel.live/library/node:22-alpine node:22-alpine
docker pull docker.1panel.live/library/nginx:alpine && docker tag docker.1panel.live/library/nginx:alpine nginx:alpine

# ③ 构建（仓库根目录）
docker build -f src/RBS.Api/Dockerfile -t rbs-api:latest .
cd web && docker build -t web-web:latest . && cd ..

# ④ 打 Harbor 标签并推送（先登录；Harbor 在 8081 端口）
docker login 192.168.10.21:8081 -u admin -p 'Harbor12345'
docker tag rbs-api:latest 192.168.10.21:8081/rbs/rbs-api:v1.0.0
docker tag web-web:latest 192.168.10.21:8081/rbs/web-web:v1.0.0
docker push 192.168.10.21:8081/rbs/rbs-api:v1.0.0
docker push 192.168.10.21:8081/rbs/web-web:v1.0.0
```

> 建议用**带版本的 tag**（如 `v1.0.0`）而不是 `latest`，方便回滚。发布流程见第 8 章。

---

## 4. 初始化控制平面（master-1 上执行）

### 4.1 准备 kubeadm 配置

```bash
sudo mkdir -p /root/k8s-init
sudo tee /root/k8s-init/kubeadm-config.yaml <<'EOF'
apiVersion: kubeadm.k8s.io/v1beta4
kind: InitConfiguration
localAPIEndpoint:
  advertiseAddress: 192.168.10.11      # ← master-1 自己的 IP
  bindPort: 6443
---
apiVersion: kubeadm.k8s.io/v1beta4
kind: ClusterConfiguration
kubernetesVersion: v1.36.3
imageRepository: registry.aliyuncs.com/google_containers   # 国内源；网络好可去掉（默认 registry.k8s.io）
controlPlaneEndpoint: "192.168.10.100:6443"                # ← 用 VIP，所有节点都连它
apiServer:
  certSANs:
    - "192.168.10.100"     # 必须加：证书要认 VIP
    - "192.168.10.11"
    - "192.168.10.12"
    - "192.168.10.13"
networking:
  podSubnet: "10.244.0.0/16"    # Calico 会用这个网段（第 4.4 步把 Calico 改成一致）
  serviceSubnet: "10.96.0.0/12"
EOF
```

### 4.2 初始化

```bash
# --upload-certs：把控制面证书上传，让另外两台 Master 用 --certificate-key 加入（HA 必选）
sudo kubeadm init --config /root/k8s-init/kubeadm-config.yaml --upload-certs | tee /root/k8s-init/init.log
```

> ⚠️ 记下 init 输出里的两段关键信息（后文 join 要用）：
> - **master join 命令**（含 `--control-plane --certificate-key`）
> - **worker join 命令**
> 如果丢了，master-1 上可再查：
> ```bash
> # 新的普通 token + 完整 join 命令
> sudo kubeadm token create --print-join-command
> # 重新上传证书拿新的 certificate-key（30 分钟过期）
> sudo kubeadm init phase upload-certs --upload-certs
> # CA 哈希
> openssl x509 -pubkey -in /etc/kubernetes/pki/ca.crt | openssl rsa -pubin -outform der 2>/dev/null | openssl dgst -sha256 -hex | sed 's/^.* //'
> ```

### 4.3 配置 kubectl（master-1）

```bash
mkdir -p $HOME/.kube
sudo cp -i /etc/kubernetes/admin.conf $HOME/.kube/config
sudo chown $(id -u):$(id -g) $HOME/.kube/config
kubectl get nodes   # 只有 master-1，状态 NotReady 是正常的（还没装 CNI）
```

### 4.4 安装 Calico（Pod 网络）

```bash
# ① 下载清单（GitHub 不通就走"镜像到 Harbor"后备）
curl -fsSL -o /root/k8s-init/calico.yaml https://raw.githubusercontent.com/projectcalico/calico/v3.29.2/manifests/calico.yaml
# ② 把默认 IP 池 192.168.0.0/16 改成与 kubeadm podSubnet 一致（10.244.0.0/16）
sed -i 's#192.168.0.0/16#10.244.0.0/16#g' /root/k8s-init/calico.yaml
# ③ 应用
kubectl apply -f /root/k8s-init/calico.yaml
# ④ 等 calico 节点就绪
kubectl wait --namespace kube-system --for=condition=ready pod -l k8s-app=calico-node --timeout=180s
kubectl get pods -n kube-system
```

> **国内网络后备（quay.io 拉不动 Calico 镜像）**：先在能联网的机器上 `docker pull docker.1panel.live/calico/calico:v3.29.2` 等所需镜像（`cni`、`node`、`kube-controllers`、`apiserver`/`typha`），改名推送到 Harbor；再把 `calico.yaml` 里的 `quay.io/calico/...` 批量替换成 `192.168.10.21:8081/rbs/calico-...` 后 apply。也可给所有节点 containerd 配 `quay.io` 的 hosts.toml 镜像源（同 3.3 的结构，`server` 换成 `https://quay.io`、host 换成可达的 quay 镜像站）。

### 4.5 加入另外 2 台 Master（master-2、master-3 上执行）

```bash
# 在 master-2 上执行（--apiserver-advertise-address 改成自己的 IP）：
sudo kubeadm join 192.168.10.100:6443 \
  --token <token> \
  --discovery-token-ca-cert-hash sha256:<ca-hash> \
  --control-plane \
  --certificate-key <certificate-key> \
  --apiserver-advertise-address 192.168.10.12

# 在 master-3 上执行（IP 改成 192.168.10.13，token 若过期重新生成）
sudo kubeadm join 192.168.10.100:6443 \
  --token <token> \
  --discovery-token-ca-cert-hash sha256:<ca-hash> \
  --control-plane \
  --certificate-key <certificate-key> \
  --apiserver-advertise-address 192.168.10.13
```

> 两个 master join 完成后，在 master-1 上：`kubectl get nodes` 应看到 3 个控制面节点 Ready（等 Calico 起来后）。
> 控制面高可用验证：停掉 master-1（`sudo reboot`），VIP 漂到 master-2，`kubectl get nodes` 仍可用。

### 4.6 加入 2 台 Worker（worker-1、worker-2 上执行）

```bash
# 去掉 --control-plane 段，就是普通 worker join（worker-1 和 worker-2 各执行一次）
sudo kubeadm join 192.168.10.100:6443 \
  --token <token> \
  --discovery-token-ca-cert-hash sha256:<ca-hash>
```

master-1 上确认：

```bash
kubectl get nodes -o wide
# 期望：master-1~3 Ready（ROLES=control-plane），worker-1、worker-2 Ready（ROLES=<none>）
```

---

## 5. 部署业务（在任意一台 Master 上用 kubectl）

### 5.1 命名空间与 Harbor 拉取凭据

```bash
kubectl create namespace rbs
kubectl -n rbs create secret docker-registry harbor-reg \
  --docker-server=192.168.10.21:8081 \
  --docker-username=admin \
  --docker-password='Harbor12345'
```

> 这是 k8s 从 Harbor 拉私有镜像的凭据。所有 Deployment 都要在 `spec.template.spec.imagePullSecrets` 里引用它（见下）。

### 5.2 部署 API（3 副本）

`k8s/production/api-deployment.yaml`（按需替换镜像 tag、数据库 IP/密码）：

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rbs-api
  namespace: rbs
spec:
  replicas: 3            # 生成侧已原子条件插入，多副本安全，见 0.5
  selector:
    matchLabels: { app: rbs-api }
  template:
    metadata:
      labels: { app: rbs-api }
    spec:
      imagePullSecrets: [ { name: harbor-reg } ]
      containers:
        - name: rbs-api
          image: 192.168.10.21:8081/rbs/rbs-api:v1.0.0
          imagePullPolicy: IfNotPresent
          ports: [ { containerPort: 8080 } ]
          env:
            - { name: ASPNETCORE_URLS, value: "http://0.0.0.0:8080" }
            - { name: ASPNETCORE_ENVIRONMENT, value: "Production" }
            - { name: ConnectionStrings__DefaultConnection,
                value: "Server=192.168.10.30,1433;Database=RBS;User Id=sa;Password=你的数据库密码;TrustServerCertificate=true;MultipleActiveResultSets=true;Max Pool Size=100;Min Pool Size=10;Connection Timeout=30;" }
          resources:
            requests: { cpu: 200m, memory: 256Mi }
            limits:   { cpu: 1000m, memory: 512Mi }
          livenessProbe:
            httpGet: { path: /swagger/v1/swagger.json, port: 8080 }
            initialDelaySeconds: 20
            periodSeconds: 15
          readinessProbe:
            httpGet: { path: /swagger/v1/swagger.json, port: 8080 }
            initialDelaySeconds: 10
            periodSeconds: 10
```

`k8s/production/api-service.yaml`：

```yaml
apiVersion: v1
kind: Service
metadata:
  name: rbs-api-svc
  namespace: rbs
spec:
  selector: { app: rbs-api }
  ports:
    - { name: http, port: 8080, targetPort: 8080 }
```

```bash
kubectl apply -f k8s/production/api-deployment.yaml -f k8s/production/api-service.yaml
kubectl wait --for=condition=ready pod -l app=rbs-api -n rbs --timeout=120s
kubectl get pods -n rbs -o wide     # 3 个 Running/1/1 Ready；NODE 列应分布到 worker-1/2
```

> 数据库连通性：readiness 探针只验 Swagger，SQL 通不通要看日志——
> `kubectl logs -n rbs -l app=rbs-api --tail=50`，出现"调度引擎启动 / Job ... 执行成功"即 SQL 已通。

### 5.3 部署 web（3 副本）

`k8s/production/web-deployment.yaml`（web 的 nginx 反代目标还是 `rbs-api-svc:8080`，集群内 DNS 解析，无需改）：

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rbs-web
  namespace: rbs
spec:
  replicas: 3
  selector:
    matchLabels: { app: rbs-web }
  template:
    metadata:
      labels: { app: rbs-web }
    spec:
      imagePullSecrets: [ { name: harbor-reg } ]
      containers:
        - name: rbs-web
          image: 192.168.10.21:8081/rbs/web-web:v1.0.0
          imagePullPolicy: IfNotPresent
          ports: [ { containerPort: 80 } ]
          resources:
            requests: { cpu: 100m, memory: 128Mi }
            limits:   { cpu: 500m, memory: 256Mi }
```

`k8s/production/web-service-nodeport.yaml`：

```yaml
apiVersion: v1
kind: Service
metadata:
  name: rbs-web-svc
  namespace: rbs
spec:
  type: NodePort
  selector: { app: rbs-web }
  ports:
    - { name: http, port: 80, targetPort: 80, nodePort: 30883 }   # 30883 兜底入口
```

```bash
kubectl apply -f k8s/production/web-deployment.yaml -f k8s/production/web-service-nodeport.yaml
kubectl get pods -n rbs -l app=rbs-web -o wide    # 3 个 Running
```

### 5.4 部署 ingress-nginx（每 worker 1 个控制器）

```bash
# ① 下载清单（GitHub 不通：用镜像源拉控制器镜像推 Harbor 后改引用，见后备）
curl -fsSL -o /root/k8s-init/ingress-nginx.yaml https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.1/deploy/static/provider/baremetal/deploy.yaml
kubectl apply -f /root/k8s-init/ingress-nginx.yaml

# ② 改造成：副本 2 + hostNetwork（监听节点 IP 的 80/443，正好每 worker 1 个控制器）
kubectl -n ingress-nginx scale deployment ingress-nginx-controller --replicas=2
kubectl -n ingress-nginx patch deployment ingress-nginx-controller --type=strategic \
  -p '{"spec":{"template":{"spec":{"hostNetwork":true,"dnsPolicy":"ClusterFirstWithHostNet"}}}}'

# ③ 等就绪
kubectl -n ingress-nginx rollout restart deployment ingress-nginx-controller
kubectl -n ingress-nginx get pods -o wide    # 2 个 Running，分别落在 worker-1、worker-2
```

> 验证入口通了：在 worker-1、worker-2 上各执行一次 `curl http://<本机IP>/`，返回 404（ingress 默认后端，说明 80 端口已被 ingress-nginx 接管）。
>
> **国内网络后备**：`registry.k8s.io/ingress-nginx/controller:v1.12.1` 与 `kube-webhook-certgen:v1.5.2` 拉不动时：
> ```bash
> # 在有网的机器上：用镜像源拉→推 Harbor
> docker pull m.daocloud.io/registry.k8s.io/ingress-nginx/controller:v1.12.1
> docker tag m.daocloud.io/registry.k8s.io/ingress-nginx/controller:v1.12.1 192.168.10.21:8081/rbs/ingress-nginx-controller:v1.12.1
> docker push 192.168.10.21:8081/rbs/ingress-nginx-controller:v1.12.1
> # （certgen 同理）
> # 然后修改清单里两处 image 引用指向 Harbor，再 apply；其余步骤不变。
> ```

### 5.5 双域名 Ingress 路由

`k8s/production/ingress.yaml`：

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: rbs-api-ingress
  namespace: rbs
spec:
  ingressClassName: nginx
  rules:
    - host: www.rbsapi.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend: { service: { name: rbs-api-svc, port: { number: 8080 } } }
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: rbs-web-ingress
  namespace: rbs
spec:
  ingressClassName: nginx
  rules:
    - host: www.rbsadmin.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend: { service: { name: rbs-web-svc, port: { number: 80 } } }
```

```bash
kubectl apply -f k8s/production/ingress.yaml
kubectl get ingress -n rbs    # 两个 Ingress ADDRESS 列有值
```

### 5.6 配置 DNS

在**内网 DNS**（或路由器/网关）加两条 A 记录（生产不要用 hosts）：

```
www.rbsapi.com   → 192.168.10.21  192.168.10.22
www.rbsadmin.com → 192.168.10.21  192.168.10.22
```

> 本机临时测试可以先加 hosts，验证通过后再正式配 DNS（hosts 里加：`192.168.10.21 www.rbsapi.com www.rbsadmin.com`）。

---

## 6. 端到端验证（master-1 上执行）

```bash
# ① 集群状态：5 个节点 Ready、核心组件健康
kubectl get nodes
kubectl get pods -A -o wide

# ② 业务 Pod：api×3 + web×3 全 Running，分布在 worker-1/2
kubectl get pods -n rbs -o wide

# ③ 从任意节点访问 API 域名（DNS 已生效时；没有 DNS 就先加 hosts）
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://www.rbsapi.com/swagger/v1/swagger.json   # 期望 200

# ④ 登录（同时验证 ingress→api→SQL 三层）
curl -s -X POST http://www.rbsapi.com/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"123456"}'

# ⑤ web 站点 + web→api 反代链路
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://www.rbsadmin.com/                          # 期望 200
curl -s -X POST http://www.rbsadmin.com/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"123456"}'

# ⑥ 兜底入口（NodePort，不经 ingress）
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://192.168.10.21:30883                       # 期望 200

# ⑦ PDF 下载（验证跨平台修复；Git Bash/PowerShell 均可）
TOKEN=$(curl -s -X POST http://www.rbsapi.com/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"123456"}' \
  | grep -oE '"token":"[^"]+"' | cut -d'"' -f4)
curl -s -H "Authorization: Bearer $TOKEN" http://www.rbsapi.com/api/Note/dndownload -o dn_test.pdf
file dn_test.pdf       # 期望：PDF document, version 1.4, 2 page(s)
```

---

## 7. 每台服务器详细步骤（独立可执行）

> 每台服务器按本节的顺序从上到下执行即可，命令已按该服务器角色精简。通用准备（1.2~1.7）每台都要做。
> 涉及 join 的 token 全部来自 master-1（见 4.2 说明，过期就重新生成）。

### 7.1 master-1（控制面初始化 + VIP + 后续运维）

```bash
# ── A. 通用准备 ──
sudo hostnamectl set-hostname master-1
# （追加 hosts：1.2 的内容；关闭 swap：1.3；内核模块：1.4；containerd：1.5；kubeadm 三件套：1.6）
# 1.7 验证：kubectl version --client && kubeadm version && containerd --version

# ── B. 高可用负载均衡（nginx + keepalived）──
# 2.1 装 nginx + 写 /etc/nginx/conf.d/k8s-apiserver-lb.conf（只绑 VIP:6443）+ ip_nonlocal_bind
# 2.2 装 keepalived + 写 /etc/keepalived/keepalived.conf（state=MASTER, priority=100）
# 2.3 验证：ip addr 里有 192.168.10.100

# ── C. 初始化控制面 ──
# 4.1 写 /root/k8s-init/kubeadm-config.yaml（endpoint=VIP, advertiseAddress=192.168.10.11）
# 4.2 sudo kubeadm init --config /root/k8s-init/kubeadm-config.yaml --upload-certs
#     记下两个 join 命令
# 4.3 配置 kubectl（cp /etc/kubernetes/admin.conf ~/.kube/config）
# 4.4 装 Calico（改 IP 池后 apply，等 calico-node Ready）

# ── D. 后续运维（master-1 是管理员常用的入口）──
kubectl get nodes -o wide                      # 全部 5 节点 Ready
kubectl get pods -A -o wide                    # 看所有组件
# 业务部署、验证见第 5、6 章
```

### 7.2 master-2（加入控制面 + VIP）

```bash
# ── A. 通用准备 ──
sudo hostnamectl set-hostname master-2
# （1.2~1.7 同 master-1，注意 hosts、版本一致）

# ── B. 高可用负载均衡 ──
# 2.1 nginx 配置与 master-1 完全一样
# 2.2 keepalived：state=BACKUP, priority=90（其余同 master-1）

# ── C. 加入控制面 ──
sudo kubeadm join 192.168.10.100:6443 \
  --token <token> --discovery-token-ca-cert-hash sha256:<ca-hash> \
  --control-plane --certificate-key <certificate-key> \
  --apiserver-advertise-address 192.168.10.12

# ── D. 验证 ──
kubectl get nodes    # （需先按 4.3 配置 ~/.kube/config；或直接在 master-1 看）
```

### 7.3 master-3（加入控制面 + VIP）

```bash
# ── A. 通用准备 ──
sudo hostnamectl set-hostname master-3
# （1.2~1.7 同前）

# ── B. 高可用负载均衡 ──
# 2.1 nginx 配置同前
# 2.2 keepalived：state=BACKUP, priority=80

# ── C. 加入控制面 ──
sudo kubeadm join 192.168.10.100:6443 \
  --token <token> --discovery-token-ca-cert-hash sha256:<ca-hash> \
  --control-plane --certificate-key <certificate-key> \
  --apiserver-advertise-address 192.168.10.13
```

### 7.4 worker-1（加入集群 + Harbor + 业务 Pod）

```bash
# ── A. 通用准备 ──
sudo hostnamectl set-hostname worker-1
# （1.2~1.7 同前；注意这台多装了 docker-ce，见 B）

# ── B. 私有仓库 Harbor ──
# 3.1 装 docker-ce + docker-compose-plugin
# 3.2 下载并 ./install.sh 装 Harbor，UI 建项目 rbs
# 3.4 构建并推送 rbs-api / web-web 镜像到 192.168.10.21:8081/rbs/
# 3.3 5 台节点都要配 containerd certs.d 信任 Harbor（别忘了本机）

# ── C. 加入集群 ──
sudo kubeadm join 192.168.10.100:6443 \
  --token <token> --discovery-token-ca-cert-hash sha256:<ca-hash>
kubectl get nodes    # 显示 Ready

# ── D. 确认业务 Pod 调度到本机 ──
kubectl get pods -n rbs -o wide | grep worker-1   # 期望有 api/web 各若干
```

### 7.5 worker-2（加入集群 + 业务 Pod）

```bash
# ── A. 通用准备 ──
sudo hostnamectl set-hostname worker-2
# （1.2~1.7 同前；不需要 docker-ce/Harbor）

# ── C. 加入集群 ──
sudo kubeadm join 192.168.10.100:6443 \
  --token <token> --discovery-token-ca-cert-hash sha256:<ca-hash>

# ── D. 确认业务 Pod 调度到本机 ──
kubectl get pods -n rbs -o wide | grep worker-2
```

---

## 8. 日常运维

### 8.1 发布新版本

```bash
# ① 构建并推送新镜像到 Harbor（在 worker-1 或开发机）
docker build -f src/RBS.Api/Dockerfile -t rbs-api:latest .
cd web && docker build -t web-web:latest . && cd ..
docker tag rbs-api:latest 192.168.10.21:8081/rbs/rbs-api:v1.1.0
docker tag web-web:latest 192.168.10.21:8081/rbs/web-web:v1.1.0
docker push 192.168.10.21:8081/rbs/rbs-api:v1.1.0
docker push 192.168.10.21:8081/rbs/web-web:v1.1.0

# ② 改 k8s/production/api-deployment.yaml（web-deployment.yaml）里的 image tag → v1.1.0
# ③ 滚动更新（镜像 tag 变了，apply 就会触发重建）
kubectl apply -f k8s/production/api-deployment.yaml -f k8s/production/web-deployment.yaml
kubectl rollout status deployment/rbs-api -n rbs
kubectl rollout status deployment/rbs-web -n rbs
```

> 若镜像 tag 不变只改了内容：改完要 `kubectl -n rbs rollout restart deployment rbs-api`（tag 相同 apply 不会重建）。

### 8.2 回滚

```bash
kubectl -n rbs rollout undo deployment/rbs-api      # 回到上一个版本
kubectl -n rbs rollout history deployment/rbs-api   # 看历史版本
```

### 8.3 排错表

| 现象 | 排查 |
|---|---|
| 节点 `NotReady` | `journalctl -u kubelet -n 50`；`systemctl status kubelet`；确认 containerd 活着 |
| Pod `ImagePullBackOff` | ① 该节点 containerd 是否信任 Harbor（3.3）；② `kubectl -n rbs get secret harbor-reg` 是否创建；③ Harbor 项目名/镜像名是否拼对 |
| 控制面 `kubectl` 连不上 | `curl -k https://192.168.10.100:6443/version` 看 VIP 6443 是否通；不通查 nginx/keepalived（2.3） |
| `www.rbsapi.com` 打不开 | ① DNS 解析到 worker-1/2？② `kubectl get pods -n ingress-nginx` 两个控制器 Running？③ `kubectl get ingress -n rbs` ADDRESS 有值？④ `curl http://192.168.10.21:30883` 兜底通不通 |
| 登录 500 / SQL 连不上 | `kubectl logs -n rbs -l app=rbs-api --tail=100` 看连接串/网络；确认 SQL 防火墙 1433 放行、允许来自节点 IP |
| web 登录报网络错误 | ① `curl http://www.rbsapi.com/api/Auth/login` 直连 API；② 进 web Pod 测反代：`kubectl -n rbs exec deploy/rbs-web -- wget -qO- http://rbs-api-svc:8080/swagger/v1/swagger.json \| head -c 200` |
| 调度任务没跑 | `kubectl -n rbs get pods -l app=rbs-api` 副本都在 Running（3 副本原子插入已保证不重复） |
| PDF 下载 500 | 见 kind 教程附录 A；确认用的是修复后的镜像（NotePdf 无 System.Drawing），必要时重建 API 镜像 |
| Harbor 登录不上 | `docker ps \| grep goharbor` 看容器；`docker logs harbor-core` 看日志；确认 80 端口未被占用 |

### 8.4 清理 / 下线

```bash
# 逐台 worker 退出集群（在要下线的节点上）
sudo kubeadm reset -f
# 在 master-1 上删节点
kubectl delete node worker-1
# 彻底清空：kubeadm reset 后按需删除 containerd 数据 /etc/cni、/var/lib/kubelet、/var/lib/etcd（生产慎用）
```

---

## 附录 A：完整命令序列（按服务器分组）

### A.1 全部 5 台（通用，2.1/2.2 仅 Master）

```bash
# 主机名、hosts、swap、内核、sysctl、containerd、kubeadm 三件套 —— 命令见第 1 章 1.2~1.7
```

### A.2 master-1

```bash
# ① nginx 6443 转发 + keepalived VIP（第 2 章）
# ② kubeadm init（第 4 章 4.1~4.4）
# ③ 装 Calico、等 Ready（4.4）
# ④ 业务部署 + 验证（第 5、6 章）
```

### A.3 master-2 / master-3

```bash
# ① nginx 转发 + keepalived（第 2 章，priority 90/80）
# ② kubeadm join --control-plane（第 4 章 4.5）
```

### A.4 worker-1

```bash
# ① 通用准备 + docker-ce + Harbor + 推镜像（第 3 章）
# ② kubeadm join（4.6）
```

### A.5 worker-2

```bash
# ① 通用准备（第 1 章）
# ② kubeadm join（4.6）
```

---

## 附录 B：与 kind 教程的命令差异对照

| 操作 | kind 教程 | 本计划（生产） |
|---|---|---|
| 建集群 | `kind create cluster --config kind-config.yaml` | `kubeadm init` + `kubeadm join` |
| 分发镜像 | `kind load docker-image` | 构建→`docker push` Harbor→节点 `containerd` 从 Harbor 拉 |
| 域名解析 | 本机 hosts → 127.0.0.1 | 内网 DNS → worker-1/2 IP |
| 数据库连接 | `host.docker.internal` + hostAliases | 连接串直接 `Server=<DB_IP>,1433` |
| API 入口 | ingress hostPort 80/443（控制面） | ingress-nginx hostNetwork 80/443（worker） |
| 管理端 | 本机 kubectl | master-1 kubectl（或任一已配 kubeconfig 的机器） |
| 升级/回滚 | `kubectl rollout restart` | 换 tag → `kubectl apply` / `rollout undo` |

> 其余业务差异点（PDF 修复、replicas=3 并发安全、探针、时区）与 kind 教程完全一致，不再重复。
