FROM saschpe/android-sdk:35-jdk17.0.18_8 AS build
USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends xz-utils curl unzip ca-certificates \
    && rm -rf /var/lib/apt/lists/*

ARG QP_SRC_00
ARG QP_SRC_01
ARG QP_SRC_02
ARG QP_SRC_03
ARG QP_SRC_04
ARG QP_SRC_05
ARG QP_SRC_06
ARG QP_SRC_07
ARG QP_SRC_08
ARG QP_SRC_09
ARG QP_SRC_10
ARG QP_SRC_11
ARG QP_SRC_12
ARG QP_SRC_13
ARG QP_SRC_14
ARG QP_SRC_15
ARG QP_SRC_16
ARG QP_SRC_17
ARG QP_SRC_COUNT
ARG QP_KEYSTORE_B64
ARG QP_KEYSTORE_PASSWORD
ARG QP_KEY_ALIAS
ARG QP_KEY_PASSWORD

RUN set -eu; \
    mkdir -p /work /public; \
    printf '%s' "$QP_SRC_00" "$QP_SRC_01" "$QP_SRC_02" "$QP_SRC_03" "$QP_SRC_04" "$QP_SRC_05" "$QP_SRC_06" "$QP_SRC_07" "$QP_SRC_08" "$QP_SRC_09" "$QP_SRC_10" "$QP_SRC_11" "$QP_SRC_12" "$QP_SRC_13" "$QP_SRC_14" "$QP_SRC_15" "$QP_SRC_16" "$QP_SRC_17" > /tmp/source.b64; \
    base64 -d /tmp/source.b64 > /tmp/source.tar.xz; \
    tar -xJf /tmp/source.tar.xz -C /work; \
    cd /work; \
    printf '%s' "$QP_KEYSTORE_B64" | base64 -d > /tmp/quickprint-release.p12; \
    chmod 600 /tmp/quickprint-release.p12; \
    export QP_KEYSTORE_PATH=/tmp/quickprint-release.p12; \
    export QP_KEYSTORE_PASSWORD QP_KEY_ALIAS QP_KEY_PASSWORD; \
    sed -i '/^org\.gradle\.jvmargs=/d;/^org\.gradle\.workers\.max=/d;/^org\.gradle\.parallel=/d' gradle.properties; \
    printf '%s\n' \
      'org.gradle.jvmargs=-Xmx1536m -XX:MaxMetaspaceSize=512m -Dfile.encoding=UTF-8' \
      'org.gradle.workers.max=1' \
      'org.gradle.parallel=false' >> gradle.properties; \
    curl -fsSL --retry 4 --retry-delay 2 https://services.gradle.org/distributions/gradle-8.10.2-bin.zip -o /tmp/gradle.zip; \
    unzip -q /tmp/gradle.zip -d /tmp; \
    rm -f /tmp/gradle.zip /tmp/source.b64 /tmp/source.tar.xz; \
    echo '[quickprint-build] Metal builder: assembling signed release'; \
    /tmp/gradle-8.10.2/bin/gradle --no-daemon --stacktrace --max-workers=1 assembleRelease validateSigningRelease; \
    APK=app/build/outputs/apk/release/app-release.apk; \
    test -s "$APK"; \
    APKSIGNER=$(find "${ANDROID_SDK_ROOT:-/opt/android-sdk-linux}/build-tools" -type f -name apksigner | sort -V | tail -n1); \
    AAPT=$(find "${ANDROID_SDK_ROOT:-/opt/android-sdk-linux}/build-tools" -type f -name aapt | sort -V | tail -n1); \
    test -x "$APKSIGNER"; test -x "$AAPT"; \
    cp "$APK" /public/QuickPrintOS-v3.3.2-signed.apk; \
    "$APKSIGNER" verify --verbose --print-certs /public/QuickPrintOS-v3.3.2-signed.apk > /public/apk-signature.txt; \
    grep -qi '29c27b8a9d6630416cbc6f366241c42fe69d6e5746d2ff768b745858a3810f71' /public/apk-signature.txt; \
    "$AAPT" dump badging /public/QuickPrintOS-v3.3.2-signed.apk | head -n1 > /public/apk-badging.txt; \
    grep -q "name='br.com.quickprint.os'" /public/apk-badging.txt; \
    grep -q "versionCode='25'" /public/apk-badging.txt; \
    grep -q "versionName='3.3.2'" /public/apk-badging.txt; \
    sha256sum /public/QuickPrintOS-v3.3.2-signed.apk > /public/apk-sha256.txt; \
    rm -f /tmp/quickprint-release.p12; \
    echo '[quickprint-build] RELEASE_VALIDATED'

FROM python:3.13-alpine
COPY --from=build /public /public
EXPOSE 8080
CMD ["sh","-c","python -m http.server ${PORT:-8080} --directory /public --bind 0.0.0.0"]
