#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8090
EXPOSE 8090

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SingleBlog/SingleBlog.csproj", "SingleBlog/"]
RUN dotnet restore "SingleBlog/SingleBlog.csproj"
COPY . .
WORKDIR "/src/SingleBlog"
RUN dotnet build "SingleBlog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SingleBlog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SingleBlog.dll"]