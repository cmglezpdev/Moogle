.PHONY: build
build:
	dotnet build

.PHONY: dev
dev:
	dotnet watch run --project MoogleServer
.PHONY: test
test:
	dotnet run --project Test
