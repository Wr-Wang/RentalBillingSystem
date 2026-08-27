# ============================================================
#  web 前端 Docker 镜像（多阶段构建）
#
#  阶段 1：用 Node 编译 Vue 3 + Vite 前端 → 产出 dist/ 静态文件
#  阶段 2：用 nginx 托管静态文件 + 反向代理 /api 到后端
#
#  构建命令（在 web/ 目录下执行）：
#    docker build -t rbs-web .
# ============================================================

# ---------- 阶段 1：构建前端静态文件 ----------
FROM node:22-alpine AS build
WORKDIR /app

# 1) 先只拷贝依赖清单，利用 Docker 层缓存：
#    package.json / package-lock.json 没变时，这一步会复用缓存的层，不用每次重新装依赖
COPY package.json package-lock.json ./

# 2) 国内网络环境：把 npm 源切到镜像（npmmirror），避免 registry.npmjs.org 超时
#    镜像与官方同步，可放心使用；`npm ci` 会按配置源下载
RUN npm config set registry https://registry.npmmirror.com

# 3) 按锁文件精确安装依赖（比 npm install 更快更确定）
RUN npm ci

# 4) 再拷贝剩余源码并构建（Vite 产物默认输出到 dist/）
COPY . .
RUN npm run build

# ---------- 阶段 2：nginx 托管 ----------
FROM nginx:alpine

# 覆盖默认站点配置（含 /api 反向代理 + history 路由回退），见 nginx.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 把阶段 1 构建出的 dist 拷进 nginx 的站点根目录
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

# 前台启动 nginx（容器主进程必须前台运行，否则容器会立即退出）
CMD ["nginx", "-g", "daemon off;"]
