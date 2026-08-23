FROM node:20-alpine AS build-env
WORKDIR /app
COPY ./ClientApp/package*.json ./
RUN npm ci
COPY ./ClientApp/ ./
RUN npm run build

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY --from=build-env /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/ || exit 1
CMD ["nginx", "-g", "daemon off;"]
