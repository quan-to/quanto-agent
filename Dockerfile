FROM mono:5.8

MAINTAINER Lucas Teske <lucas@contaquanto.com.br>

#ARG DEBIAN_FRONTEND=noninteractive
#RUN apt-get update && apt-get install -y --no-install-recommends mono-complete && rm -rf /var/lib/apt/lists/*

VOLUME ["/keys", "/opt/quanto-agent/db"]

EXPOSE "5100"
ENV HTTP_PORT "5100"
ENV PRIVATE_KEY_FOLDER /keys
ENV SYSLOG_IP "127.0.0.1"
ENV SYSLOG_FACILITY "LOG_USER"
ENV KEY_PREFIX ""
ENV MASTER_GPG_KEY_PATH ""
ENV MASTER_GPG_KEY_PASSWORD_PATH ""
ENV MASTER_GPG_KEY_BASE64_ENCODED "true"
ENV KEYS_BASE64_ENCODED "true"

RUN mkdir -p /opt/quanto-agent/
RUN mkdir -p /tmp/quanto-agent/
RUN mkdir -p /keys
RUN ln -s /lib/x86_64-linux-gnu/libc.so.6 /lib/x86_64-linux-gnu/libc.so

COPY ./ /tmp/quanto-agent
WORKDIR /tmp/quanto-agent
RUN ./build-nix.sh
RUN cp -R ./QuantoAgent/bin/Release/* /opt/quanto-agent/
WORKDIR /
RUN rm -fr /tmp/quanto-agent

CMD /usr/bin/mono /opt/quanto-agent/QuantoAgent.exe

