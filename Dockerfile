FROM saschpe/android-sdk:35-jdk17.0.18_8
USER root
RUN apt-get update && apt-get install -y --no-install-recommends xz-utils curl unzip ca-certificates && rm -rf /var/lib/apt/lists/*
COPY railway-entrypoint.sh /railway-entrypoint.sh
RUN chmod 0755 /railway-entrypoint.sh
ENTRYPOINT ["/railway-entrypoint.sh"]
