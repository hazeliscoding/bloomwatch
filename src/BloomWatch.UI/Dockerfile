# Stage 1: build Angular app
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Stage 2: serve with nginx
FROM nginx:alpine AS serve

COPY --from=build /app/dist/BloomWatch.UI/browser /usr/share/nginx/html
COPY src/BloomWatch.UI/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
