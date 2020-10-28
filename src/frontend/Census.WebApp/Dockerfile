FROM node:12 AS build-env
WORKDIR /app/out
COPY  ./ClientApp/ ./
RUN npm install && npm run build

FROM nginx:stable-alpine
COPY  --from=build-env /app/out/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]