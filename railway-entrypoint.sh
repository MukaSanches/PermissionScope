#!/usr/bin/env bash
set -euo pipefail
if [ -z "${QP_SRC_00:-}" ]; then
  echo "[builder] waiting for private source payload"
  exec sleep infinity
fi
COUNT="${QP_SRC_COUNT:-18}"
rm -rf /work/qp
mkdir -p /work/qp
: > /tmp/source.b64
for n in $(seq 0 $((COUNT-1))); do
  i=$(printf '%02d' "$n")
  v="QP_SRC_${i}"
  printf '%s' "${!v}" >> /tmp/source.b64
done
base64 -d /tmp/source.b64 > /tmp/source.tar.xz
tar -xJf /tmp/source.tar.xz -C /work/qp
cd /work/qp

# Railway builder memory guard: override the source's workstation-sized heap.
cat >> gradle.properties <<'EOF'
org.gradle.jvmargs=-Xmx512m -XX:MaxMetaspaceSize=256m -Dfile.encoding=UTF-8
org.gradle.workers.max=1
org.gradle.parallel=false
org.gradle.daemon=false
kotlin.compiler.execution.strategy=in-process
EOF

exec bash railway/railway_build.sh
