# Estágio 1: Build da aplicação Angular
FROM node:20-alpine AS build
WORKDIR /app

# Copia dependências primeiro para aproveitar o cache do Docker
COPY package*.json ./
RUN npm install

# Copia todo o código-fonte e gera o build de produção
COPY . .
RUN npm run build -- --configuration production

# Estágio 2: Servidor Web Nginx
FROM nginx:alpine
# Copia o build gerado (o Angular 17+ gera dentro de dist/<nome-do-app>/browser)
COPY --from=build /app/dist/korp-erp-web/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]