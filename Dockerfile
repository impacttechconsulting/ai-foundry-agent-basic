# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY src/AiFoundryAgent.csproj ./src/
COPY test/AiFoundryAgent.Tests.csproj ./test/
RUN dotnet restore ./src/AiFoundryAgent.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish ./src/AiFoundryAgent.csproj -c Release -o out

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/src/out .

# Expose port
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "AiFoundryAgent.dll"]