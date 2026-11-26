#!/usr/bin/env bash
# ===========================================================================
# init-db.sh — RBS 数据库一键初始化（建库 → 建表 → 基础种子 → 业务种子）
#
# 用法：
#   bash database/init-db.sh -S . -P "123456"
#   bash database/init-db.sh -S "localhost,1433" -d RBS -U sa -P "123456" --skip-seed-biz
#   bash database/init-db.sh --recreate --force -P "123456"
#
# 说明：
#   - 只对“空库”执行建表和种子数据；库中已有表时默认跳过全部初始化步骤
#     （种子脚本会重置菜单和演示房源数据，重复执行会丢数据）。
#   - 密码也可通过环境变量 SQLCMDPASSWORD 传入；不传则交互式输入。
#   - 需要 sqlcmd：Windows 随 SQL Server/ODBC 客户端安装；
#     Linux 可安装 mssql-tools18（/opt/mssql-tools18/bin/sqlcmd）。
# ===========================================================================
set -euo pipefail

SERVER="."
DATABASE="RBS"
USERNAME="sa"
PASSWORD="${SQLCMDPASSWORD:-}"
SQLCMD_BIN=""
SKIP_SEED_BIZ=0
RECREATE=0
FORCE=0

usage() {
    sed -n '2,16p' "$0" | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        -S|--server)      SERVER="${2:?缺少参数值}"; shift 2 ;;
        -d|--database)    DATABASE="${2:?缺少参数值}"; shift 2 ;;
        -U|--username)    USERNAME="${2:?缺少参数值}"; shift 2 ;;
        -P|--password)    PASSWORD="${2:?缺少参数值}"; shift 2 ;;
        --sqlcmd)         SQLCMD_BIN="${2:?缺少参数值}"; shift 2 ;;
        --skip-seed-biz)  SKIP_SEED_BIZ=1; shift ;;
        --recreate)       RECREATE=1; shift ;;
        --force)          FORCE=1; shift ;;
        -h|--help)        usage; exit 0 ;;
        *) echo "未知参数：$1" >&2; usage >&2; exit 1 ;;
    esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
SCRIPTS_DIR="$REPO_ROOT/scripts"
CREATE_SCRIPT="$SCRIPT_DIR/00_create_database.sql"
INIT_SCRIPT="$SCRIPTS_DIR/Init.sql"
SEED_BASE="$SCRIPTS_DIR/SeedBase.sql"
SEED_BIZ="$SCRIPTS_DIR/SeedBiz.sql"

# sqlcmd 是 Windows 原生程序；在 Git Bash/Cygwin 下需把 /e/... 转成 E:\...
to_native_path() {
    if command -v cygpath >/dev/null 2>&1; then
        cygpath -w "$1"
    else
        printf '%s' "$1"
    fi
}

CREATE_SCRIPT_ARG="$(to_native_path "$CREATE_SCRIPT")"
INIT_SCRIPT_ARG="$(to_native_path "$INIT_SCRIPT")"
SEED_BASE_ARG="$(to_native_path "$SEED_BASE")"
SEED_BIZ_ARG="$(to_native_path "$SEED_BIZ")"

for f in "$CREATE_SCRIPT" "$INIT_SCRIPT" "$SEED_BASE" "$SEED_BIZ"; do
    [[ -f "$f" ]] || { echo "找不到脚本：$f" >&2; exit 1; }
done

if [[ ! "$DATABASE" =~ ^[A-Za-z_][A-Za-z0-9_]*$ ]]; then
    echo "数据库名不合法：$DATABASE（只能包含字母、数字、下划线，且不能以数字开头）" >&2
    exit 1
fi

# ---- 定位 sqlcmd ----------------------------------------------------------
if [[ -z "$SQLCMD_BIN" ]]; then
    if command -v sqlcmd >/dev/null 2>&1; then
        SQLCMD_BIN="$(command -v sqlcmd)"
    elif [[ -x /opt/mssql-tools18/bin/sqlcmd ]]; then
        SQLCMD_BIN=/opt/mssql-tools18/bin/sqlcmd
    elif [[ -x /opt/mssql-tools/bin/sqlcmd ]]; then
        SQLCMD_BIN=/opt/mssql-tools/bin/sqlcmd
    fi
fi
[[ -n "$SQLCMD_BIN" ]] || {
    echo "未找到 sqlcmd。请安装 mssql-tools18，或用 --sqlcmd 指定完整路径。" >&2
    exit 1
}

# ---- 密码 -----------------------------------------------------------------
if [[ -z "$PASSWORD" ]]; then
    read -r -s -p "请输入 SQL Server 用户 [$USERNAME] 的密码: " PASSWORD
    echo
fi
[[ -n "$PASSWORD" ]] || { echo "密码不能为空。" >&2; exit 1; }
export SQLCMDPASSWORD="$PASSWORD"

sqlcmd_run() {
    local db="$1"; shift
    "$SQLCMD_BIN" -S "$SERVER" -U "$USERNAME" -d "$db" \
        -b -r1 -f 65001 -C -I "$@"
}

table_count() {
    local out
    out="$(sqlcmd_run "$DATABASE" -h -1 -W \
        -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables WHERE type='U';")"
    out="${out//[!0-9]/}"
    echo "${out:-0}"
}

# ---- 主流程 ---------------------------------------------------------------
echo "============================================"
echo " RBS 数据库初始化"
echo " 服务器 : $SERVER"
echo " 数据库 : $DATABASE"
echo " 登录名 : $USERNAME"
echo "============================================"

if [[ "$RECREATE" -eq 1 ]]; then
    if [[ "$FORCE" -ne 1 ]]; then
        read -r -p "警告：将删除数据库 [$DATABASE] 及其全部数据。输入 yes 继续: " answer
        [[ "$answer" == "yes" ]] || { echo "已取消。"; exit 0; }
    fi
    echo
    echo ">>> 删除数据库 [$DATABASE]"
    sqlcmd_run master -Q "
IF DB_ID(N'$DATABASE') IS NOT NULL
BEGIN
    ALTER DATABASE [$DATABASE] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$DATABASE];
END"
fi

# 1. 建库
echo
echo ">>> 00_create_database.sql  →  数据库 [master]"
sqlcmd_run master -v "DatabaseName=$DATABASE" -i "$CREATE_SCRIPT_ARG"

# 2. 建表 + 种子数据（仅当库为空时执行）
count="$(table_count)"
if [[ "$count" -gt 0 ]]; then
    echo
    echo ">>> [$DATABASE] 已有 $count 张表，判定为已初始化。"
    echo "    跳过 Init.sql / SeedBase.sql / SeedBiz.sql（种子脚本会重置菜单和房源数据）。"
    echo "    如需彻底重建，请加 --recreate 参数。"
else
    echo
    echo ">>> Init.sql  →  数据库 [$DATABASE]"
    sqlcmd_run "$DATABASE" -i "$INIT_SCRIPT_ARG"

    echo
    echo ">>> SeedBase.sql  →  数据库 [$DATABASE]"
    sqlcmd_run "$DATABASE" -i "$SEED_BASE_ARG"

    if [[ "$SKIP_SEED_BIZ" -eq 1 ]]; then
        echo
        echo ">>> 已指定 --skip-seed-biz，跳过 SeedBiz.sql。"
    else
        echo
        echo ">>> SeedBiz.sql  →  数据库 [$DATABASE]"
        sqlcmd_run "$DATABASE" -i "$SEED_BIZ_ARG"
    fi
fi

echo
echo "============================================"
echo " 初始化完成：[$DATABASE] 共有 $(table_count) 张表"
echo " 默认登录：admin / 123456（超级管理员）"
echo " 其余种子账号见 database/README.md"
echo "============================================"
