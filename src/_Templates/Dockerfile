# Dockerfile
FROM postgres:18.2

# (опционально) можно задать дефолтные переменные прямо тут
ENV POSTGRES_USER=test \
    POSTGRES_PASSWORD=test \
    POSTGRES_DB=test

# (опционально) если хочешь авто-инициализацию SQL при первом старте:
# COPY ./init.sql /docker-entrypoint-initdb.d/01-init.sql

