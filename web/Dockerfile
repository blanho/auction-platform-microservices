FROM node:22-alpine@sha256:c610fcdfb1d5b4740dd70c284ed3cb16bb857e0f7166196e36a5501df7a3aa32 AS build
WORKDIR /app
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ .
ARG VITE_API_URL=/api
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

FROM nginx:1.27-alpine@sha256:65645c7bb6a0661892a8b03b89d0743208a18dd2f3f17a54ef4b76fb8e2f2a10 AS runtime
ARG BUILD_VERSION=dev
ARG VCS_REF=unknown
LABEL org.opencontainers.image.version=$BUILD_VERSION \
      org.opencontainers.image.revision=$VCS_REF
COPY web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
RUN sed -i \
      -e '/^user  nginx;$/d' \
      -e 's#pid        /run/nginx.pid;#pid /tmp/nginx.pid;#' \
      -e 's#/var/log/nginx/error.log#/dev/stderr#' \
      -e 's#/var/log/nginx/access.log#/dev/stdout#' \
      /etc/nginx/nginx.conf && \
    chown -R nginx:nginx /var/cache/nginx /var/run /usr/share/nginx/html
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD wget -q --spider http://localhost:8080/ || exit 1
USER nginx
CMD ["nginx", "-g", "daemon off;"]
