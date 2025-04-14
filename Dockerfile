# Sử dụng image từ .NET SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Thiết lập thư mục làm việc
WORKDIR /app

# Sao chép csproj và restore các gói nuget
COPY *.csproj ./
RUN dotnet restore

# Sao chép tất cả các file còn lại và build ứng dụng
COPY . ./
RUN dotnet publish -c Release -o out

# Sử dụng image .NET Runtime để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /app/out .

# Cấu hình cổng mà ứng dụng sẽ sử dụng (port 80 là mặc định cho HTTP)
EXPOSE 80

# Chạy ứng dụng khi container được chạy
ENTRYPOINT ["dotnet", "TodoApp.dll"]
