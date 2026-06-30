# ---- Build Stage ----
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
# Permite pasar la URL de la API en tiempo de compilación (Vite requiere esto al compilar)
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---- Production Stage ----
FROM nginx:alpine
# Copiar el build compilado de React
COPY --from=build /app/dist /usr/share/nginx/html
# Copiar el template de nginx para la inyección del puerto dinámico de Railway ($PORT)
COPY default.conf.template /etc/nginx/templates/default.conf.template
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
