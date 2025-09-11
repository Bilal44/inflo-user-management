# Stop and remove docker containers
docker compose down

# Remove images
docker rmi inflo-um-client-img
docker rmi inflo-um-api-img
docker rmi inflo-um-web-img
docker rmi mcr.microsoft.com/mssql/server:2019-latest
