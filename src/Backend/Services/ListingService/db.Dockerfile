# Dockerfile
FROM postgres:18.2

ENV POSTGRES_USER=test
ENV POSTGRES_PASSWORD=test
ENV POSTGRES_DB=test

# COPY ./init.sql /docker-entrypoint-initdb.d/01-init.sql

