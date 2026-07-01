# ---- Build Stage ----
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files using wildcards to handle both root and subdirectory contexts
COPY package*.json frontend/package*.jso[n] ./

RUN npm ci

# Copy the rest of the files into a temporary directory
COPY . /tmp/project/

# Move the frontend files to the working directory depending on the context
RUN if [ -d /tmp/project/frontend ]; then \
        cp -r /tmp/project/frontend/. ./; \
    else \
        cp -r /tmp/project/. ./; \
    fi && rm -rf /tmp/project

# Permite pasar la URL de la API en tiempo de compilación (Vite requiere esto al compilar)
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---- Production Stage ----
FROM nginx:alpine
# Copiar el build compilado de React
COPY --from=build /app/dist /usr/share/nginx/html
# Copiar el template de nginx para la inyección del puerto dinámico de Railway ($PORT) desde la etapa de compilación
COPY --from=build /app/default.conf.template /etc/nginx/templates/default.conf.template
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
