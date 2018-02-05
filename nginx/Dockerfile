FROM nginx:alpine
COPY nginx.conf /etc/nginx/nginx.conf
RUN mkdir -p /usr/local/ssl
WORKDIR /usr/local/ssl
COPY ./nginx-selfsigned.crt .
COPY ./nginx-selfsigned.key .