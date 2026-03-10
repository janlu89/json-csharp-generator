# ---- Build Stage ----
# Node 20 Alpine is a lightweight Linux variant — Alpine Linux is popular in
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

# Copy source and build the Angular app for production.
COPY . .
RUN npm run build

# ---- Runtime Stage ----
FROM nginx:alpine
COPY --from=build /app/dist/client/browser /usr/share/nginx/html
EXPOSE 80