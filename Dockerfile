# ========== BUILD STAGE ==========
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files first (for layer caching)
COPY TicketFlow.Domain/TicketFlow.Domain.csproj TicketFlow.Domain/
COPY TicketFlow.Application/TicketFlow.Application.csproj TicketFlow.Application/
COPY TicketFlow.Infrastructure/TicketFlow.Infrastructure.csproj TicketFlow.Infrastructure/
COPY TicketFlow.API/TicketFlow.API.csproj TicketFlow.API/

# Restore dependencies
RUN dotnet restore TicketFlow.API/TicketFlow.API.csproj

# Copy source directories
COPY TicketFlow.Domain/ TicketFlow.Domain/
COPY TicketFlow.Application/ TicketFlow.Application/
COPY TicketFlow.Infrastructure/ TicketFlow.Infrastructure/
COPY TicketFlow.API/ TicketFlow.API/

# Build and publish
RUN dotnet publish TicketFlow.API/TicketFlow.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ========== RUNTIME STAGE ==========
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser -d /app -s /sbin/nologin appuser

EXPOSE 8080

COPY --from=build /app/publish .

RUN chown -R appuser:appuser /app

USER appuser

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/swagger/index.html || exit 1

ENTRYPOINT ["dotnet", "TicketFlow.API.dll"]
