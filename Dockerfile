# ── Stage 1: Build ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for optimal layer caching on restore
COPY PRN232.LMS.Models/PRN232.LMS.Models.csproj             PRN232.LMS.Models/
COPY PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj PRN232.LMS.Repositories/
COPY PRN232.LMS.Services/PRN232.LMS.Services.csproj          PRN232.LMS.Services/
COPY PRN232.LMS.API/PRN232.LMS.API.csproj                    PRN232.LMS.API/

RUN dotnet restore PRN232.LMS.API/PRN232.LMS.API.csproj

# Copy all source and publish
COPY . .
RUN dotnet publish PRN232.LMS.API/PRN232.LMS.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Listen on port 8080 inside the container (mapped to host port in compose)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll"]
