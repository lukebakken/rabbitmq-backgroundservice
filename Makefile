.PHONY: all build format

DOTNET_CLI_TELEMETRY_OPTOUT=1

all: build format

format:
	dotnet format

build:
	dotnet build
