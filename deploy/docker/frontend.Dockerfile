# syntax=docker/dockerfile:1

FROM node:24-alpine AS build
WORKDIR /app

COPY src/frontend/project-management-saas/package.json src/frontend/project-management-saas/package-lock.json src/frontend/project-management-saas/.npmrc ./
RUN npm ci

COPY src/frontend/project-management-saas/index.html ./
COPY src/frontend/project-management-saas/tsconfig.json src/frontend/project-management-saas/tsconfig.app.json src/frontend/project-management-saas/tsconfig.node.json ./
COPY src/frontend/project-management-saas/vite.config.ts ./
COPY src/frontend/project-management-saas/src src/

ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

FROM nginx:stable-alpine AS runtime
COPY deploy/docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
