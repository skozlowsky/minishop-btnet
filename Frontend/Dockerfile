FROM node:23.8-alpine

WORKDIR /app
COPY package.json .

RUN npm install
RUN npm i -g serve

COPY . .

RUN npm run build

EXPOSE 3000

ENTRYPOINT sh -c "./dist/vite-envs.sh && serve -s dist"