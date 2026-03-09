# Dockerfile
FROM postgres:18.2

ENV POSTGRES_USER=postgres \
    POSTGRES_PASSWORD=1234 \
    POSTGRES_DB=userservice-db

# COPY ./init.sql /docker-entrypoint-initdb.d/01-init.sql

