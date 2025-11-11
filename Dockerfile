# ---------------------------
# Stage 1: build
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy everything
COPY . .

# Restore and publish the project (adjust project path if different)
RUN dotnet restore "CDM.HRManagement/CDM.HRManagement.csproj"
RUN dotnet publish "CDM.HRManagement/CDM.HRManagement.csproj" -c Release -o /app/out --no-restore

# ---------------------------
# Stage 2: runtime
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# Copy published output from build stage
COPY --from=build /app/out ./

# Expose port (optional; container listens on 80 by default)
EXPOSE 80

# The published DLL name normally matches the project name; ensure it is correct
ENTRYPOINT ["dotnet", "CDM.HRManagement.dll"]
