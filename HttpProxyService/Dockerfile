#See https://aka.ms/containerfastmode to understand how Visual Studio uses this 
#
ARG RABBITMQ_HOST_ARG=host.docker.internal
ARG 1CHTTP_HOST_ARG=http://host.docker.internal/Institute/hs/student_info
#
# Build .NET Project with MSBuilder tools (сборка с использованием инструмента)
FROM mcr.microsoft.com/dotnet/sdk AS build
#
# Docker container Application Metadatas (Метаданные приложения)
LABEL APPLICATION_NAME=LEARNER_RECORD
WORKDIR /src
#
COPY /HttpProxyService/. HttpProxyService/
COPY /TransferLibrary/. TransferLibrary/
#
RUN dotnet restore "HttpProxyService/HttpProxyService.csproj"
WORKDIR "/src/HttpProxyService"
#
RUN dotnet build "HttpProxyService.csproj" -c Release -o /app/build
RUN dotnet publish "HttpProxyService.csproj" -c Release -o /app/publish \
	/p:UseAppHost=false
#
# Compile and Run .NET Assembly (запуск созданной сборки)
FROM mcr.microsoft.com/dotnet/runtime:6.0 As base
WORKDIR /app
#
ENV 1CHTTP_HOST=1CHTTP_HOST_ARG
ENV RABBITMQ_HOST=RABBITMQ_HOST_ARG
#
# Environment Variables for Docker Container APP
ENV HANDLER_TIMER=5000
ENV HTTP_TIMEOUT=5000
#
ENV INPUT_PATH=InputQueue
ENV OUTPUT_PATH=OutputExchange
#
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HttpProxyService.dll"]
EXPOSE 8000/tcp