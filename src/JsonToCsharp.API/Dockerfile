# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project and solution files FIRST, before copying any source code.
COPY JsonToCsharp.slnx ./
COPY JsonToCsharp.Engine/JsonToCsharp.Engine.csproj JsonToCsharp.Engine/
COPY JsonToCsharp.API/JsonToCsharp.API.csproj JsonToCsharp.API/
COPY JsonToCsharp.Tests/JsonToCsharp.Tests.csproj JsonToCsharp.Tests/
RUN dotnet restore

# Now copy all source code and publish the API project in Release configuration.
COPY . .
RUN dotnet publish JsonToCsharp.API/JsonToCsharp.API.csproj -c Release -o /out

# ---- Runtime Stage ----
# The ASP.NET runtime image is much smaller (~200MB) because it contains only
# what's needed to *run* the app, not build it.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy only the compiled output from the build stage — nothing else.
COPY --from=build /out .
ENTRYPOINT ["dotnet", "JsonToCsharp.API.dll"]